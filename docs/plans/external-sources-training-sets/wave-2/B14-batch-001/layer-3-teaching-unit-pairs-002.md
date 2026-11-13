# Layer 3 — Teaching Unit Pairs (002)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## T002-001 — "design" fires ARCHITECTURE universally

Rule: "design" + structural noun → ARCHITECTURE
  "design the infrastructure" → ARCHITECTURE
  "design the service layer" → ARCHITECTURE
  "design the API" → ARCHITECTURE (if structural context) | API_CONTRACT (if contract/schema context)
  Teaching: "design" is the canonical ARCHITECTURE trigger verb.

---

## T002-002 — "architect" (verb) fires ARCHITECTURE

Rule: "architect" as verb → ARCHITECTURE
  "architect the platform" → ARCHITECTURE
  "architect the migration" → ARCHITECTURE
  Teaching: "architect" as a verb is equivalent to "design" for ARCHITECTURE firing.

---

## T002-003 — Cloud platform names are gap

Rule: AWS, Azure, GCP, Kubernetes, EKS, ECS, S3, GKE → gap
  "move to AWS" → gap | "configure Azure" → gap | "use Kubernetes" → gap
  Teaching: Platform names do not fire ARCHITECTURE. They are infrastructure vocabulary gap.

---

## T002-004 — ARCHITECTURE sub-phase 3.5 vs 3.1 disambiguation

Rule: Infra vocabulary → 3.5; App/domain vocabulary → 3.1
  Infra vocab: infrastructure, topology, cluster, networking, container, VPC, service mesh → 3.5
  App vocab: domain model, service, module, pattern, dependency (app-level) → 3.1
  Teaching: Same ARCHITECTURE predicate, different sub-phase based on vocabulary co-occurrence.

---

## T002-005 — "scale" as operational verb is gap

Rule: "scale" (operational action) → gap
  "scale up the database" → gap | "scale the pods" → gap
  vs "design the scaling architecture" → ARCHITECTURE
  Teaching: "scale" without architecture framing is gap. The design of scaling strategy = ARCHITECTURE.

