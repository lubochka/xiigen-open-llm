# Layer 0-D — Flow Factory Pairs (016)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-016-001 — decision-log update: knowledge_edge

**Input:** "Update the decision log with all key decisions from the steering committee."

**Compiler output:** []

**Flow factory decision:**
- Zero predicates → knowledge_edge activated
- SCHEDULE_CAPABILITY_LEARNING emitted
- Gap IDs: gap-project-managing-manage-delivery-001, gap-project-managing-accountability-001

---

## B37-0D-016-002 — decision-log with DOCUMENT + ARCHITECTURE: two factories

**Input:** "Document all architectural decisions in the decision log."

**Compiler output:** ["DOCUMENT", "ARCHITECTURE"]

**Flow factory decision:**
- DOCUMENT → DocumentationFlow (2.1)
- ARCHITECTURE → ArchitectureFlow31 (3.1)
- Both factories selected (parallel eligible)
- DomainId: "software-development"
- Gap warnings: gap-project-managing-manage-delivery-001

---

## B37-0D-016-003 — decision-log with QA: ReviewFlow

**Input:** "Review the decision log before the programme board."

**Compiler output:** ["QA"]

**Flow factory decision:**
- QA → ReviewFlow (QA sub-phase)
- DomainId: "software-development"
- Gap warnings: gap-project-managing-manage-delivery-001

---

## B37-0D-016-004 — decision-log with DOCUMENT: DocumentationFlow

**Input:** "Define the decision-making process and log all decisions."

**Compiler output:** ["DOCUMENT"]

**Flow factory decision:**
- DOCUMENT → DocumentationFlow (2.1)
- DomainId: "software-development"
- Gap warnings: gap-project-managing-manage-delivery-001

---

## B37-0D-016-005 — DecisionLogFlow does NOT exist (v6-01 boundary)

**Confirmed absent from factory registry:**
- DecisionLogFlow — does not exist
- DecisionManagementFlow — does not exist
- GovernanceDecisionFlow — does not exist
- ActionDecisionFlow — does not exist

For zero-predicate decision log requests: knowledge_edge (no factory).
For predicate-firing requests: DocumentationFlow, ArchitectureFlow31, ReviewFlow — as determined by predicate scan only.
