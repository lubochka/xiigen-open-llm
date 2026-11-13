# Layer 0-D — Flow Factory Pairs (004)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: flow-factory-pairs

---

## B37-0D-004-001 — ARCHITECTURE+DOCUMENT factory: ArchitectureFlow31 + DocumentationFlow

**Input:** "Define the accountability structure for the project delivery team."

**Compiler output:** ["ARCHITECTURE", "DOCUMENT"]

**Flow factory decision:**
- ARCHITECTURE (structure) → ArchitectureFlow31 (sub-phase 3.1)
- DOCUMENT (define) → DocumentationFlow (sub-phase 2.1)
- Both factories selected in parallel
- DomainId: "software-development"

**Factory pair:** ArchitectureFlow31 + DocumentationFlow
**Parallel eligible:** Yes (independent flows, no ordering dependency)

**Gap warnings forwarded:**
- gap-project-managing-accountability-001
- gap-project-managing-manage-delivery-001

---

## B37-0D-004-002 — ARCHITECTURE+DOCUMENT factory: governance structure

**Input:** "Document the governance structure and define the accountability framework."

**Compiler output:** ["ARCHITECTURE", "DOCUMENT"]

**Flow factory decision:**
- ARCHITECTURE (structure) → ArchitectureFlow31
- DOCUMENT (document, define) → DocumentationFlow
- Parallel: ArchitectureFlow31 + DocumentationFlow
- DomainId: "software-development"

**Gap warnings:**
- gap-project-managing-accountability-001

---

## B37-0D-004-003 — ARCHITECTURE+DOCUMENT+QA factory: three flows

**Input:** "Review the governance structure and define the escalation criteria."

**Compiler output:** ["ARCHITECTURE", "DOCUMENT", "QA"]

**Flow factory decision:**
- ARCHITECTURE (structure) → ArchitectureFlow31
- DOCUMENT (define, criteria) → DocumentationFlow
- QA (review) → QAGatingFlow
- All three parallel eligible
- DomainId: "software-development"

**Gap warnings:**
- gap-project-managing-escalation-001

---

## B37-0D-004-004 — ARCHITECTURE-only factory: "structure" without DOCUMENT

**Input:** "Establish the escalation structure for critical blockers."

**Compiler output:** ["ARCHITECTURE"]

**Flow factory decision:**
- ARCHITECTURE (structure) → ArchitectureFlow31 only
- No DOCUMENT → DocumentationFlow not selected
- DomainId: "software-development"

**Gap warnings:**
- gap-project-managing-escalation-001
- gap-project-managing-blockers-001

---

## B37-0D-004-005 — Factory parallel eligibility rules

**B37 factory parallel eligibility:**

| Factory combination | Parallel eligible? | Notes |
|--------------------|-------------------|-------|
| ArchitectureFlow31 + DocumentationFlow | Yes | Independent flows |
| ArchitectureFlow31 + QAGatingFlow | Yes | Independent |
| DocumentationFlow + QAGatingFlow | Yes | Independent |
| BackendServiceFlow41 + DocumentationFlow | Yes | Independent |
| BackendServiceFlow41 + ArchitectureFlow31 | Yes | Architecture may inform service build |
| TestFlow5x + QAGatingFlow | Ordered | Test before QA gating |
| TestFlow5x + DocumentationFlow | Yes | Independent |
| APIContractFlow33 + DocumentationFlow | Yes | Independent |

**Rule:** All B37 factory combinations are parallel eligible EXCEPT TestFlow + QAGatingFlow which requires test before gating.
