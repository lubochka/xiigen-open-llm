# Layer 0-A — Compiler Pairs (018)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-018-001 — ARCHITECTURE: DR topology

A: "Design the disaster recovery topology — active-passive failover, RTO (Recovery Time Objective) < 15 minutes, RPO (Recovery Point Objective) < 5 minutes."
  "design" → ARCHITECTURE | "disaster recovery topology" → ARCHITECTURE ("topology") | "active-passive failover" → ARCHITECTURE ("pattern") | "RTO", "RPO" → ARCHITECTURE (recovery design constraints)
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "active-passive", "15 minutes", "5 minutes" → operational specification vocabulary, gap

---

## B14-compiler-018-002 — ARCHITECTURE+DOCUMENT: DR runbook

A: "Architect the disaster recovery failover procedure and document the runbook as an operational guide."
  "architect" → ARCHITECTURE | "disaster recovery failover procedure" → ARCHITECTURE | "document" → DOCUMENT | "runbook" → DOCUMENT
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
  Gap: "runbook" → operational document type, gap (but "document" fires DOCUMENT)

---

## B14-compiler-018-003 — ARCHITECTURE: backup topology

A: "Define the backup infrastructure topology — snapshot schedules, off-site replication, and restoration verification pipeline."
  "define" → ARCHITECTURE | "backup infrastructure topology" → ARCHITECTURE ("topology", "infrastructure") | "pipeline" → ARCHITECTURE ("layer")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "snapshot schedules", "off-site replication" → operational configuration vocabulary, gap

---

## B14-compiler-018-004 — ARCHITECTURE+IMPLEMENT: DR IaC

A: "Design the multi-region DR architecture and implement the Terraform failover configuration."
  "design" → ARCHITECTURE | "multi-region DR architecture" → ARCHITECTURE | "implement" → IMPLEMENT | "Terraform failover configuration" → IMPLEMENT
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  Gap: "Terraform", "multi-region DR" → tool/operational vocabulary, gap

---

## B14-compiler-018-005 — gap: "test the DR failover"

A: "Test the disaster recovery failover by promoting the replica."
  "test" → TEST | "disaster recovery failover" → ARCHITECTURE? — "disaster recovery" alone without design context; "test" is the primary action
  Boundary: "test the DR failover" → TEST fires on "test". ARCHITECTURE does not fire without design vocabulary.
  CompiledPredicateIds: ["TEST"]
  Teaching: "test" fires TEST. DR vocabulary alone does not fire ARCHITECTURE without design/architect/define framing.
