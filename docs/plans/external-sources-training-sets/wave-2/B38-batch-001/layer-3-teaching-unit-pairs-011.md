# Layer 3 — Teaching Unit Pairs (011)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B38-TU-011-001 — Teaching: Predicate vocabulary disambiguation — QA vs IMPLEMENT vs GAP

LESSON: Many words in release gate context could plausibly fire multiple predicates. This unit shows the correct classification for ambiguous vocabulary.

---

## B38-TU-011-002 — Disambiguation table: release vocabulary

| Term | Fires | Does NOT fire | Reason |
|---|---|---|---|
| review | QA | IMPLEMENT, DOCUMENT | QA lexicon core |
| approve | QA | IMPLEMENT | QA lexicon core |
| sign-off | QA | IMPLEMENT | QA lexicon core (rule 23) |
| audit | QA (+SECURITY if security context) | IMPLEMENT | QA core; SECURITY by rule 19 |
| service | IMPLEMENT | QA | Rule 2 absolute |
| build | IMPLEMENT | QA, DOCUMENT | Rule 6 absolute |
| pipeline | IMPLEMENT | QA | Code artifact |
| criteria | DOCUMENT | QA, IMPLEMENT | Rule 3 absolute |
| deploy / deployment | GAP | QA, IMPLEMENT | Rule 4 |
| rollout | GAP | QA, IMPLEMENT | Rule 5 |
| go-live | GAP | QA, IMPLEMENT | Rule 5 |
| launch | GAP | IMPLEMENT | Rule 5 |
| monitor | GAP | QA, IMPLEMENT | Rule 7 |
| execute | GAP | IMPLEMENT | Rule 8 |
| checklist | GAP | DOCUMENT | Rule 5 (checklist ≠ criteria) |

---

## B38-TU-011-003 — Key disambiguations that must be memorized

"checklist" is NOT "document/criteria":
- checklist → GAP
- criteria → DOCUMENT
These look similar (both are lists of items) but fire differently.

"deploy/deployment" is NOT "build":
- deployment → GAP
- build → IMPLEMENT
Both relate to software delivery but fire differently.

"execute" is NOT "implement":
- execute → GAP
- build/service/pipeline → IMPLEMENT
Running is not writing.

"audit" context matters for SECURITY:
- audit alone → QA
- audit + security → QA + SECURITY
