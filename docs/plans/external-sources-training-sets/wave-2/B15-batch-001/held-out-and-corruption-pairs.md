# Held-Out and Corruption Pairs
# Batch: B15-batch-001
# Scenario: B15 — Development from scratch — backend order processing service
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05

---

## Held-Out B15B1-HO-001 — "function" fires IMPLEMENT

```
source_id: xiigen-b15-ho-001
expected_outcome: IMPLEMENT fires on function/write/logic
```

A: "Write a function that calculates the total order amount including taxes and discounts."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  Routing: "4.1"
  Predicate analysis: "write" → IMPLEMENT; "function" → IMPLEMENT; "calculates" → not in lexicon directly;
  "logic" (implied) — not explicitly present; but "write" + "function" give strong IMPLEMENT signal.

---

## Held-Out B15B1-HO-002 — "develop" fires IMPLEMENT

```
source_id: xiigen-b15-ho-002
expected_outcome: IMPLEMENT fires on develop/module
```

A: "Develop the inventory management module and integrate it with the order service."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  Routing: "4.1"
  CapabilityGapNotes: "integrate — not in IMPLEMENT directly; but 'module' and 'develop' fire IMPLEMENT"
  Note: "integrate" — could fire API_CONTRACT if co-occurring with API terms; here it does not.
  Predicate analysis: "develop" → IMPLEMENT; "module" → IMPLEMENT. Result: ["IMPLEMENT"].

---

## Held-Out B15B1-HO-003 — "write" on test description still fires IMPLEMENT (and TEST)

```
source_id: xiigen-b15-ho-003
expected_outcome: IMPLEMENT + TEST both fire
```

A: "Write the unit tests for the OrderProcessingService and implement the missing edge cases."

B:
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  Routing: "4.1 (IMPLEMENT) + 5.1 (TEST)"
  Predicate analysis: "write" → IMPLEMENT; "implement" → IMPLEMENT; "unit tests" → TEST fires on "unit" and "test";
  "edge cases" — "case" → TEST fires on "case".
  DecisionCount: 2.

---

## Corruption B15B1-COR-001 — v6-01: IMPLEMENT suppressed, "order" misread as gap

```
source_id: xiigen-b15-cor-001
corruption_type: "Corruption-v6-01: knowledge edge suppression"
```

CORRUPT B: CompiledPredicateIds: [] for "Build an order processing service from scratch."
Corrupt rationale given: "'order processing' is domain terminology; no predicate matched."

Why corrupt: "build" fires IMPLEMENT; "service" fires IMPLEMENT; "logic" fires IMPLEMENT.
The corrupt model confused "order processing" (domain terminology) with "order" (a non-predicate word)
and incorrectly concluded no predicate fired. "Build" and "service" are unambiguous IMPLEMENT terms.

Correct B: CompiledPredicateIds: ["IMPLEMENT"], Confidence: 0.92.

---

## Corruption B15B1-COR-002 — v6-01: "model" → ARCHITECTURE suppressed

```
source_id: xiigen-b15-cor-002
corruption_type: "Corruption-v6-01: partial knowledge edge — ARCHITECTURE suppressed"
```

CORRUPT B: "Write the order service: implement the domain model, define the data contracts."
→ CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"] (ARCHITECTURE and API_CONTRACT missing)

Why corrupt: "model" fires ARCHITECTURE and "contract" fires API_CONTRACT. The corrupt model
correctly fired IMPLEMENT and DOCUMENT but suppressed the other two predicates. All four must fire.

Correct B: CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT", "API_CONTRACT", "ARCHITECTURE"].

---

## Corruption B15B1-COR-003 — v6-03: four routing decisions collapsed to one

```
source_id: xiigen-b15-cor-003
corruption_type: "Corruption-v6-03: multi-branch result collapse"
```

CORRUPT B: DecisionCount: 1, only 4.1 returned for [IMPLEMENT, DOCUMENT, API_CONTRACT, ARCHITECTURE].

Why corrupt: Four predicates fired. The router must produce four independent decisions.
Selecting only the "dominant" IMPLEMENT decision is the v6-03 corruption: collapsing
multi-branch results to a single routing. The model must output DecisionCount: 4.

Correct B: DecisionCount: 4 — 4.1 (IMPLEMENT), 2.1 (DOCUMENT), 3.3 (API_CONTRACT), 3.1 (ARCHITECTURE).
