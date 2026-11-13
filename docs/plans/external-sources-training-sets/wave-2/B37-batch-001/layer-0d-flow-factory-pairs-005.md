# Layer 0-D — Flow Factory Pairs (005)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: flow-factory-pairs

---

## B37-0D-005-001 — QA factory: QAGatingFlow (review in managing context)

**Input:** "Review the escalation log and follow up with the engineering team on open action items."

**Compiler output:** ["QA"]

**Flow factory decision:**
- QA (review) → QAGatingFlow or ReviewFlow
- Default selection in review context: ReviewFlow (not full QAGatingFlow for project review)
- DomainId: "software-development"
- Factory: ReviewFlow

**Gap warnings forwarded:**
- gap-project-managing-escalation-001
- gap-project-managing-follow-up-001
- gap-project-managing-action-items-001

**Factory note:** ReviewFlow is selected for "review" of non-code artifacts (logs, documents, plans). QAGatingFlow is selected when the review is part of a testing gate. In the escalation log context, ReviewFlow is appropriate.

---

## B37-0D-005-002 — QA factory: code review context (same token, different artifact)

**Input:** "Review the code quality and track the open defects."

**Compiler output:** ["QA"]

**Flow factory decision:**
- QA (review) → QAGatingFlow (code quality context)
- DomainId: "software-development"
- Factory: QAGatingFlow

**Gap warnings:**
- gap-project-managing-track-001 ("track the defects" = managing intent, not TEST)

**Factory distinction note:** "review code quality" → QAGatingFlow. "review escalation log" → ReviewFlow. The factory selects the variant based on artifact type. Both fire QA on "review".

---

## B37-0D-005-003 — QA factory: validate context

**Input:** "Validate the delivery timeline against the project baseline."

**Compiler output:** ["QA"]

**Flow factory decision:**
- QA (validate) → QAGatingFlow
- DomainId: "software-development"

**Gap warnings:**
- gap-project-managing-manage-delivery-001

---

## B37-0D-005-004 — QA factory with DOCUMENT: combined factory selection

**Input:** "Review the escalation criteria and document the follow-up procedures."

**Compiler output:** ["QA", "DOCUMENT"]

**Flow factory decision:**
- QA (review) → ReviewFlow
- DOCUMENT (document, criteria) → DocumentationFlow
- Parallel: ReviewFlow + DocumentationFlow
- DomainId: "software-development"

**Gap warnings:**
- gap-project-managing-escalation-001
- gap-project-managing-follow-up-001

---

## B37-0D-005-005 — QA factory absent: "check" does not fire QA

**Input:** "Check the delivery progress and confirm blockers are being managed."

**Compiler output:** []

**Analysis:**
- "Check" → not in QA lexicon. QA fires on: review, validate (and testing keywords for TEST)
- "delivery" → gap
- "progress" → gap (Rule 12)
- "blockers" → gap
- "managed" → gap (Rule 10: manage family)

Result: []

**Flow factory decision:**
- Zero predicates → factory silent → knowledge_edge
- DomainId: "project-management"

**Gap warnings:**
- gap-project-managing-manage-delivery-001
- gap-project-managing-progress-001
- gap-project-managing-blockers-001

Note: "check" does NOT fire QA. Only "review" and "validate" fire QA from common managing vocabulary.
