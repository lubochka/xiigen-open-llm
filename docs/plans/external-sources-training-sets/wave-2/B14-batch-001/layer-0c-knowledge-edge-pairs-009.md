# Layer 0-C — Knowledge Edge Pairs (009)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E009-001 — "anti-affinity" fires ARCHITECTURE in scheduling context

A: "Define the pod anti-affinity rules to distribute replicas across availability zones."
  "define" → ARCHITECTURE | "pod anti-affinity rules" → ARCHITECTURE ("rules", structural scheduling constraint) | "availability zones" → ARCHITECTURE (infrastructure boundary)
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "anti-affinity" fires ARCHITECTURE as a Kubernetes scheduling architecture concept.

---

## E009-002 — "rate limiting" fires ARCHITECTURE when designed (not just mentioned)

A: "Design the rate limiting architecture for the public API — define per-consumer quotas and burst thresholds."
  "design" → ARCHITECTURE | "rate limiting architecture" → ARCHITECTURE | "define" → ARCHITECTURE | "per-consumer quotas" → ARCHITECTURE ("quotas" as design parameters) | "burst thresholds" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "rate limiting" fires ARCHITECTURE when inside a design-framing sentence. "Add rate limiting" → IMPLEMENT; "design the rate limiting architecture" → ARCHITECTURE.

---

## E009-003 — "IaC" (Infrastructure as Code) fires ARCHITECTURE+IMPLEMENT

A: "Implement the IaC (Infrastructure as Code) pipeline using Terraform and validate the module outputs."
  "implement" → IMPLEMENT | "IaC pipeline" → ARCHITECTURE ("IaC" as infrastructure architectural pattern) + IMPLEMENT | "validate" → TEST
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "TEST"]
  Teaching: "IaC" fires ARCHITECTURE. The full form "Infrastructure as Code" also fires ARCHITECTURE. "implement the IaC pipeline" → ARCHITECTURE+IMPLEMENT.

---

## E009-004 — "SLA" (Service Level Agreement) alone is gap; "design for SLA" fires ARCHITECTURE

A: "Define the SLA (Service Level Agreement) requirements: 99.9% uptime, <200ms p95 response time."
  "define" → ARCHITECTURE? → in SLA requirements context: borderline
  SLA requirements definition alone → gap (business requirement, not architecture design)
  vs "Design the infrastructure architecture to meet 99.9% SLA requirements."
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: SLA requirements alone are gap. "Design the architecture to meet SLA" fires ARCHITECTURE.

---

## E009-005 — "disaster recovery" fires ARCHITECTURE in design context

A: "Architect the disaster recovery (DR) topology — active-active cross-region with RPO (Recovery Point Objective) under 1 minute."
  "architect" → ARCHITECTURE | "disaster recovery topology" → ARCHITECTURE | "active-active cross-region" → ARCHITECTURE (structural pattern)
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "disaster recovery" in architecture design context fires ARCHITECTURE. RPO/RTO alone are operational metrics; they are gap without design verb framing.

