# Layer 0-A — Compiler Pairs (007)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: compiler

---

## B14-compiler-007-001 — ARCHITECTURE: storage tier

A: "Design the storage tier architecture — define the object storage, block storage, and file storage layers and their access patterns."
  "design" → ARCHITECTURE | "storage tier architecture" → ARCHITECTURE | "define" → ARCHITECTURE | "object storage, block storage, file storage layers" → ARCHITECTURE ("layers") | "access patterns" → ARCHITECTURE ("patterns")
  CompiledPredicateIds: ["ARCHITECTURE"]

---

## B14-compiler-007-002 — ARCHITECTURE: blue/green deployment

A: "Structure the blue/green deployment architecture — define the traffic routing rules and rollback boundary conditions."
  "structure" → ARCHITECTURE | "blue/green deployment architecture" → ARCHITECTURE | "define" → ARCHITECTURE | "traffic routing rules" → ARCHITECTURE ("rules") | "rollback boundary conditions" → ARCHITECTURE ("boundary")
  CompiledPredicateIds: ["ARCHITECTURE"]

---

## B14-compiler-007-003 — ARCHITECTURE: multi-cloud

A: "Architect the multi-cloud architecture — define cloud-agnostic abstraction layers between the application and the cloud provider APIs."
  "architect" → ARCHITECTURE | "multi-cloud architecture" → ARCHITECTURE | "define" → ARCHITECTURE | "cloud-agnostic abstraction layers" → ARCHITECTURE ("layers", "abstraction") | "cloud provider APIs" → API_CONTRACT ("APIs")
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT"]

---

## B14-compiler-007-004 — ARCHITECTURE+DOCUMENT: ADR

A: "Document the architecture decision records (ADRs) for the infrastructure choices made during the platform migration."
  "document" → DOCUMENT | "architecture decision records" → ARCHITECTURE ("architecture", "decision records") | "ADRs" → ARCHITECTURE (abbreviation for Architecture Decision Records) | "infrastructure choices" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]

---

## B14-compiler-007-005 — gap: "migrate to AWS"

A: "Migrate our application to AWS."
  "migrate" → gap (without "design the migration architecture") | "AWS" → gap (platform name) | "application" → gap (without architecture framing)
  CompiledPredicateIds: [] → gap
  Teaching: "migrate" alone is gap. "architect the migration topology" or "design the migration strategy" fires ARCHITECTURE.

