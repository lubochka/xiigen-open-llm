# Layer 0-C — Knowledge Edge Pairs (043) — MergeSort: gap analysis for sort and merge domain verbs
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E043-001 — "merge" does NOT fire IMPLEMENT

"Merge the two sorted sub-arrays produced by the MergeSort divide phase." → CompiledPredicateIds: []
"merge" is a domain operation verb. It is NOT in the IMPLEMENT predicate lexicon.
Contrast: "merge" as a git operation also does not fire predicates in B20 context.
IMPLEMENT fires only on: write, create, build, develop, implement, add, extend, generate.

---

## E043-002 — "start the sort" execution gap: reinforced for MergeSort

"Start the MergeSort on the input collection before the batch job completes." → CompiledPredicateIds: []
"start" → execution verb; zero predicates.
Confirmed: all six execution verbs (run, execute, trigger, launch, invoke, start) universally produce zero predicates.

---

## E043-003 — "stable sort" domain term does NOT fire TEST

"Ensure the MergeSort implements stable sort behaviour." → CompiledPredicateIds: []
"ensure" → informal QA; zero predicates. "stable sort" → domain algorithmic property term; does not fire TEST.
Combined: both gap signals in one request → zero predicates.

---

## E043-004 — "test fixture" fires TEST; context verb does not suppress it

"Set up a test fixture with large randomised arrays for MergeSort benchmarking." → CompiledPredicateIds: ["TEST"]
"test fixture" → TEST (compound). "set up" is context verb; does not suppress the TEST signal.
TransitionType: documentation (no construction verb present in the request).
