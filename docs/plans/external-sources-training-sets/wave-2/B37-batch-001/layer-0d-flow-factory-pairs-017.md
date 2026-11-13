# Layer 0-D — Flow Factory Pairs (017)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-017-001 — lessons-learned session: knowledge_edge

**Input:** "Run the lessons-learned session at project end and collect feedback."

**Compiler output:** []

**Flow factory decision:**
- Zero predicates → knowledge_edge activated
- SCHEDULE_CAPABILITY_LEARNING emitted
- Gap IDs: gap-project-managing-work-stream-001, gap-project-managing-manage-delivery-001

---

## B37-0D-017-002 — lessons-learned with DOCUMENT: DocumentationFlow

**Input:** "Document lessons learned from the delivery programme."

**Compiler output:** ["DOCUMENT"]

**Flow factory decision:**
- DOCUMENT → DocumentationFlow (2.1)
- DomainId: "software-development"
- Gap warnings: gap-project-managing-manage-delivery-001

---

## B37-0D-017-003 — lessons-learned action items: knowledge_edge

**Input:** "Track lessons-learned action items and follow up with owners."

**Compiler output:** []

**Flow factory decision:**
- Zero predicates → knowledge_edge activated
- SCHEDULE_CAPABILITY_LEARNING emitted
- Gap IDs: gap-project-managing-track-001, gap-project-managing-action-items-001, gap-project-managing-follow-up-001, gap-project-managing-accountability-001

---

## B37-0D-017-004 — lessons-learned with QA: ReviewFlow

**Input:** "Review lessons learned and validate improvement actions."

**Compiler output:** ["QA"]

**Flow factory decision:**
- QA → ReviewFlow / QAGatingFlow (QA sub-phase)
- DomainId: "software-development"
- Gap warnings: gap-project-managing-action-items-001

---

## B37-0D-017-005 — LessonsLearnedFlow does NOT exist (v6-01 boundary)

**Confirmed absent from factory registry:**
- LessonsLearnedFlow — does not exist
- RetrospectiveFlow — does not exist
- PostImplementationFlow — does not exist
- LearningCaptureFlow — does not exist

For zero-predicate lessons-learned requests: knowledge_edge (no factory).
Lessons-learned is a capability gap; no flow exists for retrospective management.
