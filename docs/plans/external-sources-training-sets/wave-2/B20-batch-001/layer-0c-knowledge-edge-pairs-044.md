# Layer 0-C — Knowledge Edge Pairs (044) — QuickSort: gap analysis for pivot and partition domain verbs
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E044-001 — "partition" does NOT fire IMPLEMENT

"Partition the array around the chosen pivot element using QuickSort logic." → CompiledPredicateIds: []
"partition" is a domain operation verb. It is NOT in the IMPLEMENT predicate lexicon.
Domain partition operations are outside all predicate lexicons.

---

## E044-002 — "invoke the quick sort routine" execution gap

"Invoke the QuickSort routine on the unsorted dataset." → CompiledPredicateIds: []
"invoke" → execution verb; zero predicates.
Reinforces the universal rule: all six execution verbs produce zero predicates for any algorithm domain.

---

## E044-003 — "worst-case input" domain phrase does NOT fire TEST

"Ensure the QuickSort handles worst-case input (already sorted) gracefully." → CompiledPredicateIds: []
"ensure" → informal QA. "worst-case input" → domain algorithmic analysis term; does not fire TEST.
The SCHEDULE_CAPABILITY_LEARNING: true applies because the request is about algorithm behaviour.

---

## E044-004 — "spec pivot selection" fires TEST; subject matter is irrelevant

"Spec the QuickSort pivot selection behaviour for worst-case input." → CompiledPredicateIds: ["TEST"]
Note: this is NOT a zero-predicate case. "spec" is in the TEST predicate lexicon.
The subject matter (pivot selection, worst-case) does not change the TEST firing.
TransitionType: documentation (no IMPLEMENT verb present).
