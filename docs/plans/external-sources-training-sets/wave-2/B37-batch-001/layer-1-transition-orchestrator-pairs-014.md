# Layer 1 — Transition Orchestrator Pairs (014)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-014-001 — change-request: TransitionType unknown

**Input:** "Process the change request and coordinate approval."

**Compiler output:** []

**Transition orchestrator:**
- TransitionType: unknown
- ContextCarryForward: false
- Next state: knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING: emitted
- Gap IDs: gap-project-managing-coordinate-001, gap-project-managing-manage-delivery-001

---

## B37-L1-014-002 — change-request with DOCUMENT: TransitionType documentation

**Input:** "Document the change request with stakeholder sign-off."

**Compiler output:** ["DOCUMENT"]

**Transition orchestrator:**
- TransitionType: documentation
- Next state: DocumentationFlow (2.1)
- DomainId: "software-development"
- Gap IDs: gap-project-managing-manage-delivery-001, gap-project-managing-stakeholder-update-001

---

## B37-L1-014-003 — change-request with QA: TransitionType qa_review

**Input:** "Review pending change requests in the change log."

**Compiler output:** ["QA"]

**Transition orchestrator:**
- TransitionType: qa_review
- Next state: ReviewFlow (QA sub-phase)
- DomainId: "software-development"
- Gap IDs: gap-project-managing-manage-delivery-001

---

## B37-L1-014-004 — change-request with IMPLEMENT: TransitionType feature_addition

**Input:** "Implement the approved change request for the user service."

**Compiler output:** ["IMPLEMENT"]

**Transition orchestrator:**
- TransitionType: feature_addition
- Next state: BackendServiceFlow41 (4.1)
- DomainId: "software-development"
- Gap IDs: gap-project-managing-manage-delivery-001

---

## B37-L1-014-005 — v6 ordering violation for change-request: DomainId set before scan

**v6 ordering violation:** DomainId "project-management" set at ingestion because request "looks like" change management.

**Scenario:**
Input: "Review the change request for the authentication service."
Correct: Review fires QA → DomainId "software-development".
v6 corrupt: DomainId "project-management" set because "change request" is managing vocabulary.

**Why wrong:** DomainId must be set AFTER compiler scan. "review" fires QA. QA fires → DomainId "software-development". The managing framing of "change request" does not override the predicate scan result.

Correct processing order:
1. Compiler scan: "review" → QA fires
2. DomainId set: "software-development" (QA predicate)
3. Route: ReviewFlow
