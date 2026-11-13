# Held-Out and Corruption Pairs (022)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-022-001 — HO: managing + DOCUMENT (stakeholder) fires correctly

**Request:** "Track blockers, coordinate escalations, and update stakeholders on delivery status."

**Expected output:**
- "Track" → gap
- "blockers" → gap
- "coordinate" → not in predicate lexicon
- "escalations" → gap
- "update" → not in DOCUMENT lexicon
- "stakeholders" → DOCUMENT fires (Rule 6)
- "delivery status" → gap
- Predicates: ["DOCUMENT"]
- DomainId: "software-development"
- Route: DocumentationFlow (2.1)
- Gap IDs: track-001, blockers-001, coordinate-001, escalation-001, stakeholder-update-001, manage-delivery-001

---

## B37-HO-022-002 — HO: managing without stakeholder → zero predicates

**Request:** "Track blockers, coordinate escalations, and update the programme board on delivery status."

**Expected output:**
- "Track" → gap
- "blockers" → gap
- "coordinate" → not in predicate lexicon
- "escalations" → gap
- "programme board" → NOT stakeholder → DOCUMENT does NOT fire
- "delivery status" → gap
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted

---

## B37-HO-022-003 — COR: managing + stakeholder → DomainId "project-management" (v6-03)

**Request:** "Produce progress report for stakeholders."

**v6-03 corrupt output:** Predicates: ["DOCUMENT"], DomainId: "project-management"

**Why this is wrong:** DOCUMENT fired. ANY predicate → DomainId "software-development" (Rule 14). DomainId cannot be "project-management" when any predicate fires.

**Correct output:** Predicates: ["DOCUMENT"], DomainId: "software-development"

---

## B37-HO-022-004 — COR: managing pile-up → multiple predicates invented (v6-01)

**Request:** "Track blockers, coordinate escalations, follow up on action items, update stakeholders."

**v6-01 corrupt output:** Predicates: ["DOCUMENT", "MANAGEMENT", "COORDINATION"]

**Why this is wrong:** MANAGEMENT and COORDINATION do not exist as predicates. Only DOCUMENT fires (on "stakeholders"). v6-01 is inventing non-existent predicates.

**Correct output:** Predicates: ["DOCUMENT"] only. DomainId "software-development".

---

## B37-HO-022-005 — HO: managing + DOCUMENT certification summary

| Case | Request | Expected predicates | Expected DomainId |
|------|---------|-------------------|-----------------|
| HO-022-001 | managing + stakeholders | [DOCUMENT] | software-development |
| HO-022-002 | managing + programme board | [] | project-management |
| HO-022-003 | managing + escalate + stakeholders | [DOCUMENT] | software-development |
| HO-022-004 | managing + coordinate + stakeholders | [DOCUMENT] | software-development |
| HO-022-005 | managing + no stakeholder | [] | project-management |

All five must pass. Zero v6 corruptions permitted.
