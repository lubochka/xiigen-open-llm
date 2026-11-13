# Layer 3 — Teaching Unit Pairs (009)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## T009-001 — "tier" fires ARCHITECTURE in structural design

Rule: "tier" as structural partition noun → ARCHITECTURE
  "design the three-tier architecture" → ARCHITECTURE
  "define the data tier" → ARCHITECTURE
  Teaching: "tier" (presentation tier, application tier, data tier) fires ARCHITECTURE as a structural partition.

---

## T009-002 — "mesh" fires ARCHITECTURE in service mesh context

Rule: "service mesh", "mesh topology" → ARCHITECTURE
  "architect the service mesh" → ARCHITECTURE
  Teaching: "mesh" fires ARCHITECTURE as a structural topology concept.

---

## T009-003 — "observability" fires ARCHITECTURE in infra stack design

Rule: "observability stack/infrastructure" + design verb → ARCHITECTURE
  "architect the observability stack" → ARCHITECTURE
  "set up observability" → gap ("set up" is operational)
  Teaching: "observability" fires ARCHITECTURE when designing the observability infrastructure. Operational setup requests are gap.

---

## T009-004 — Multiple ARCHITECTURE triggers → one ARCHITECTURE predicate

Rule: Multiple "design/architect/topology/blueprint" tokens → still one ARCHITECTURE predicate
  "Design the topology and architect the replication boundaries" → ["ARCHITECTURE"] (not ["ARCHITECTURE", "ARCHITECTURE"])
  Teaching: Predicates are a set. Multiple triggers for the same predicate → one predicate, one flow.

---

## T009-005 — "serverless" fires ARCHITECTURE in design context

Rule: "serverless architecture" + design verb → ARCHITECTURE
  "design the serverless architecture" → ARCHITECTURE
  "use serverless functions" → gap ("use" + style name is gap)
  Teaching: "serverless" fires ARCHITECTURE when inside "design the serverless architecture". Style-name alone is gap.

