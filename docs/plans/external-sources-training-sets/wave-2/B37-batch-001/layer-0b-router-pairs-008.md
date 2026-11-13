# Layer 0-B — Router Pairs (008)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: router-pairs

---

## B37-0B-008-001 — [TEST, DOCUMENT, QA] → 5.x + 2.1 + QA sub-phase

**Input:** "Test the delivery estimates with the stakeholders and validate the timeline assumptions."

**Compiler output:** ["TEST", "DOCUMENT", "QA"]

**Router decision:**
- TEST (test) → 5.x (TestFlow)
- DOCUMENT (stakeholders) → 2.1 (DocumentationFlow)
- QA (validate) → QA sub-phase
- DomainId: "software-development"
- Routes: 5.x + 2.1 + QA sub-phase (parallel eligible)

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001 (delivery estimates intent)
- gap-project-managing-track-status-001 (timeline intent)

**Router note:** Three predicates fired from one B37 managing request. The intent was project managing but the tokens "test", "stakeholders", and "validate" each fired a predicate. All three managing gap warnings persist alongside predicate routing.

---

## B37-0B-008-002 — [TEST, DOCUMENT] → 5.x + 2.1: test + stakeholder

**Input:** "Test the estimates with stakeholders before finalizing the project plan."

**Compiler output:** ["TEST", "DOCUMENT"]

**Router decision:**
- TEST (test) → 5.x
- DOCUMENT (stakeholders) → 2.1
- DomainId: "software-development"
- Routes: 5.x + 2.1

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001 (delivery plan intent)

---

## B37-0B-008-003 — [TEST, QA] → 5.x + QA sub-phase: test + validate

**Input:** "Test and validate the current delivery pace against the project baseline."

**Compiler output:** ["TEST", "QA"]

**Router decision:**
- TEST (test) → 5.x
- QA (validate) → QA sub-phase
- DomainId: "software-development"
- Routes: 5.x + QA sub-phase

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001

---

## B37-0B-008-004 — [TEST] → 5.x only: test with no other predicates

**Input:** "Test whether the current coordination process is working effectively."

**Compiler output:** ["TEST"]

**Router decision:**
- TEST (test) → 5.x
- DomainId: "software-development"
- Route: 5.x

**Gap warnings forwarded:**
- gap-project-managing-coordinate-001

---

## B37-0B-008-005 — [TEST, DOCUMENT, QA, IMPLEMENT] → four routes: extreme compound

**Input:** "Test the service module, validate the interface against the spec, and document the stakeholder requirements."

**Compiler output:** ["TEST", "DOCUMENT", "QA", "IMPLEMENT"]

**Router decision:**
- TEST (test) → 5.x
- DOCUMENT (document, spec, requirements, stakeholder) → 2.1
- QA (validate) → QA sub-phase
- IMPLEMENT (service, module) → 4.1
- DomainId: "software-development"
- Routes: 5.x + 2.1 + QA sub-phase + 4.1

**Gap warnings forwarded:** none (all vocabulary captured by predicates)

**Router note:** This is a maximally dense predicate sentence. Four predicates fire from a single request. All routes activate in parallel.
