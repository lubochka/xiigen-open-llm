# Layer 0-D — Flow Factory Pairs (014)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-014-001 — change-request coordination: knowledge_edge

**Input:** "Process the change request and coordinate approval."

**Compiler output:** []

**Flow factory decision:**
- Zero predicates → knowledge_edge activated
- SCHEDULE_CAPABILITY_LEARNING emitted
- Gap IDs: gap-project-managing-coordinate-001, gap-project-managing-manage-delivery-001

---

## B37-0D-014-002 — change-request with DOCUMENT: DocumentationFlow

**Input:** "Document the change request with impact analysis and stakeholder sign-off."

**Compiler output:** ["DOCUMENT"]

**Flow factory decision:**
- DOCUMENT predicate → DocumentationFlow (2.1)
- DomainId: "software-development"
- Gap warnings: gap-project-managing-manage-delivery-001, gap-project-managing-stakeholder-update-001

---

## B37-0D-014-003 — change-request with QA: ReviewFlow

**Input:** "Review pending change requests in the change log."

**Compiler output:** ["QA"]

**Flow factory decision:**
- QA predicate → ReviewFlow (QA sub-phase)
- DomainId: "software-development"
- Gap warnings: gap-project-managing-manage-delivery-001

---

## B37-0D-014-004 — change-request with IMPLEMENT: BackendServiceFlow41

**Input:** "Implement the approved change request to add authentication to the user service."

**Compiler output:** ["IMPLEMENT"]

**Flow factory decision:**
- IMPLEMENT predicate → BackendServiceFlow41 (4.1)
- DomainId: "software-development"
- Gap warnings: gap-project-managing-manage-delivery-001

---

## B37-0D-014-005 — ChangeRequestFlow does NOT exist (v6-01 boundary)

**Confirmed absent from factory registry:**
- ChangeRequestFlow — does not exist
- ChangeManagementFlow — does not exist
- ChangeControlFlow — does not exist
- ChangeAdvisoryFlow — does not exist

The change request is a managing artefact. No flow factory for change management exists.

For zero-predicate change request requests: knowledge_edge (no factory).
For predicate-firing change request requests: predicate-appropriate factory (DocumentationFlow, ReviewFlow, BackendServiceFlow41, etc.).
