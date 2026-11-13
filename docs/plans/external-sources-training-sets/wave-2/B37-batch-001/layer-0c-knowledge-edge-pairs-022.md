# Layer 0-C — Knowledge-Edge Pairs (022)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-022-001 — Mixed: managing + DOCUMENT false positive analysis

**Knowledge edge: managing vocabulary + "stakeholder" pattern**

When a sentence contains both B37 managing vocabulary AND "stakeholder", DOCUMENT fires on "stakeholder" and the managing vocabulary becomes gap warnings.

**Pattern:** managing_vocab + stakeholder → ["DOCUMENT"] + CapabilityGapWarnings

| Managing vocab | Stakeholder trigger | Result |
|---------------|-------------------|--------|
| track progress + stakeholders | DOCUMENT fires | ["DOCUMENT"] |
| coordinate escalations + stakeholders | DOCUMENT fires | ["DOCUMENT"] |
| blockers + stakeholders | DOCUMENT fires | ["DOCUMENT"] |
| follow up + stakeholders | DOCUMENT fires | ["DOCUMENT"] |
| status report + stakeholders | DOCUMENT fires | ["DOCUMENT"] |

In every case: DomainId "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B37-0C-022-002 — Mixed: managing + no stakeholder → zero predicates

**Knowledge edge: managing vocabulary without stakeholder fires nothing**

The same managing sentences without "stakeholder" produce zero predicates:

| Input (without stakeholder) | Result |
|---------------------------|--------|
| "Track progress and report to programme board." | [] |
| "Coordinate escalations with the delivery team." | [] |
| "Resolve blockers for the steering committee." | [] |
| "Follow up with delivery leads." | [] |
| "Send status report to the project office." | [] |

All five: DomainId "project-management", knowledge_edge, SCL emitted.

**Teaching point:** The presence or absence of "stakeholder" is the single token that determines whether DOCUMENT fires and routing changes from knowledge_edge to 2.1.

---

## B37-0C-022-003 — Gap warning count in managing + DOCUMENT sentences

**Knowledge edge: gap warnings accumulate from all managing vocabulary**

In a sentence with managing vocabulary + "stakeholder":
- Each B37 gap token generates its own gap warning
- DOCUMENT routes to 2.1
- All gap warnings forwarded alongside routing

Example: "Track blockers, coordinate escalations, follow up on action items, and report status to stakeholders."

Gap warnings generated:
1. gap-project-managing-track-001 (from "track")
2. gap-project-managing-blockers-001 (from "blockers")
3. gap-project-managing-coordinate-001 (from "coordinate")
4. gap-project-managing-escalation-001 (from "escalations")
5. gap-project-managing-follow-up-001 (from "follow up")
6. gap-project-managing-action-items-001 (from "action items")
7. gap-project-managing-status-report-001 (from "status")
8. gap-project-managing-stakeholder-update-001 (from "stakeholders" + intent)

ONE predicate ["DOCUMENT"], EIGHT gap warnings.

---

## B37-0C-022-004 — v6 corruption for managing + DOCUMENT sentences

**v6-03 corruption risk:** Setting DomainId "project-management" because the intent is managing.

**Why this is wrong:** "stakeholders" fired DOCUMENT. ANY predicate → DomainId "software-development". The managing framing does not override the predicate scan result.

**v6-02 corruption risk:** Suppressing DOCUMENT because "the sentence is about status reporting, not documentation."

**Why this is wrong:** Rule 6 is absolute. DOCUMENT fires on "stakeholder" regardless of intent.

---

## B37-0C-022-005 — Five canonical mixed-signal test cases

1. "Weekly progress report for stakeholders." → [DOCUMENT] — 2.1, DomainId "software-development"
2. "Escalate blockers to stakeholders." → [DOCUMENT] — 2.1
3. "Track action items and update stakeholders." → [DOCUMENT] — 2.1
4. "Coordinate delivery and send status to programme board." → [] — knowledge_edge
5. "Coordinate delivery and send status to stakeholders." → [DOCUMENT] — 2.1

Cases 1-3 and 5: DOCUMENT fires on "stakeholders".
Case 4: "programme board" ≠ stakeholder — zero predicates.
