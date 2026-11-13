# Layer 3 — Teaching-Unit Pairs (012)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: teaching-unit-pairs

---

## B37-L3-012-001 — Complete B37 teaching summary: 12 key points

**Point 1: track ≠ TEST**
"track" fires nothing. TEST fires on: test, verify, assert, coverage, acceptance, unit, regression, e2e.

**Point 2: report ≠ DOCUMENT**
"status report" fires nothing. DOCUMENT fires on: document, spec, define, describe, requirement, story, criteria, stakeholder, functional, behavior, use-case.

**Point 3: stakeholder → DOCUMENT always**
Even in notification/managing sentences. False positive. Gap warning captures intent.

**Point 4: structure → ARCHITECTURE always**
Even in governance/org sentences. False positive. Gap warning captures intent.

**Point 5: dependencies → API_CONTRACT only with API co-occurrence**
Team/project dependencies → gap. API/endpoint/integration dependencies → API_CONTRACT.

**Point 6: manage, coordinate, progress, escalate → zero predicates**
None are in any predicate lexicon.

**Point 7: review → QA always; test → TEST always**
Even in managing context. False positives handled with gap warnings.

**Point 8: DomainId binary switch**
Zero predicates → "project-management". Any predicate → "software-development".

**Point 9: B36 gap domain ≠ B37 gap domain**
B36 = "project-planning"; B37 = "project-managing". Same DomainId, different gap sub-domain.

**Point 10: v6 corruptions — v6-01, v6-02, v6-03**
Never invent predicates. Never suppress DOCUMENT for stakeholder. Never set wrong DomainId.

**Point 11: SCHEDULE_CAPABILITY_LEARNING**
One per zero-predicate request. Not emitted when any predicate fires.

**Point 12: Layer 2 = capability-gap-extended-pairs**
No nine-node flow for B37. Extended pairs document gap traces and mixed-signal handling.

---

## B37-L3-012-002 — Complete B37 teaching exam: 10 questions

Q1: "Track delivery progress." — predicates?
A: []

Q2: "Update stakeholders on status." — predicates?
A: ["DOCUMENT"]

Q3: "Define the accountability structure." — predicates?
A: ["ARCHITECTURE", "DOCUMENT"]

Q4: "API dependencies between services." — predicates?
A: ["API_CONTRACT"]

Q5: "Team dependencies blocking delivery." — predicates?
A: []

Q6: "Review the RAID log." — predicates?
A: ["QA"]

Q7: "Test the delivery estimates." — predicates?
A: ["TEST"]

Q8: "Build the tracking module." — predicates?
A: ["IMPLEMENT"]

Q9: "Coordinate blockers and track progress." — predicates?
A: []

Q10: DomainId for Q3?
A: "software-development" (ARCHITECTURE and DOCUMENT fired)

---

## B37-L3-012-003 — B37 false positive summary table

| Token | Predicate fired | Intent in B37 managing context | Gap warning |
|-------|---------------|-------------------------------|------------|
| stakeholder | DOCUMENT | Notification/update | stakeholder-update-001 |
| structure | ARCHITECTURE | Organizational governance | accountability-001 |
| review | QA | Managing review | varies |
| test | TEST | Evaluation/estimation check | manage-delivery-001 |
| build | IMPLEMENT | Delivery reference | manage-delivery-001 |
| service | IMPLEMENT | Service delivery (not software) | manage-delivery-001 |
| module | IMPLEMENT | Delivery component reference | manage-delivery-001 |
| define | DOCUMENT | Governance/process definition | varies |
| criteria | DOCUMENT | Escalation/acceptance criteria | escalation-001 |
| requirements | DOCUMENT | Process requirements | varies |

---

## B37-L3-012-004 — B37 gap vocabulary quick-reference

Zero-predicate tokens in B37:
track, coordinate, blockers, escalate/escalation, accountability, follow-up, action items, on-track, off-track, RAG-status, RAID log, progress, status report, manage, delivery (standalone), workstream, programme, steering, PMO

---

## B37-L3-012-005 — B37 teaching unit certification statement

**A model certified on B37-batch-001 demonstrates:**

1. Correct zero-predicate handling for all B37 managing vocabulary
2. Correct SCHEDULE_CAPABILITY_LEARNING emission
3. Correct gap ID assignment (gap-project-managing-* domain)
4. Correct false positive handling (fire + gap warning, no suppression)
5. Correct DomainId computation from predicate scan
6. Correct B36 vs B37 gap domain distinction
7. Correct API co-occurrence rule for "dependencies"
8. Zero v6-01, v6-02, v6-03 corruption instances

Certification metric: Pass all 10 exam questions (B37-L3-012-002) with zero v6 corruptions.
