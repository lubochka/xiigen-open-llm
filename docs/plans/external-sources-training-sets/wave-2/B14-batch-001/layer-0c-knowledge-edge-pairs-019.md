# Layer 0-C — Knowledge Edge Pairs (019)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-019-001 — "multi-region" deployment pattern fires ARCHITECTURE

A: "Design the multi-region deployment pattern — primary region active, secondary region warm standby, with automated failover."
  "design" → ARCHITECTURE | "multi-region deployment pattern" → ARCHITECTURE ("pattern", "deployment") | "automated failover" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "deployment pattern" and "deployment" (in design context) fire ARCHITECTURE.

---

## B14-edge-019-002 — "data residency" boundary fires ARCHITECTURE

A: "Define the data residency boundary topology — EU data must not leave eu-west-1 and us-east-1 nodes."
  "define" → ARCHITECTURE | "data residency boundary topology" → ARCHITECTURE ("boundary", "topology")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "boundary" is an ARCHITECTURE structural noun. "data residency" provides architectural constraint context.

---

## B14-edge-019-003 — "replicate to region" is gap

A: "Replicate the S3 bucket to the us-west-2 region."
  "replicate" → gap (operational action) | "S3 bucket" → gap | "us-west-2 region" → gap
  CompiledPredicateIds: []
  Gap signal: gap-devops-cloud-001 — "replicate" as a direct operational action is gap; "S3" is platform vocabulary.
  Teaching: To fire ARCHITECTURE: "architect the multi-region replication topology for the data layer."

---

## B14-edge-019-004 — "Anycast routing" fires ARCHITECTURE

A: "Architect the global Anycast routing topology for the DNS (Domain Name System) layer."
  "architect" → ARCHITECTURE | "Anycast routing topology" → ARCHITECTURE ("topology") | "DNS layer" → ARCHITECTURE ("layer")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "Anycast routing" is an architectural pattern. "topology" and "layer" reinforce ARCHITECTURE.
