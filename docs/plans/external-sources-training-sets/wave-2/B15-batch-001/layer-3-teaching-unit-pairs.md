# Layer 3 — Teaching Unit Pairs
# Batch: B15-batch-001
# Scenario: B15 — Development from scratch — backend order processing service
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: teaching_unit

---

## Pair B15B1-WDA-teach-001 — "contract" fires API_CONTRACT even in "data contracts"

```
source_id: xiigen-b15-teach-001
pair_kind: teaching_unit
```

A: "Implement the domain model and define the data contracts." Which predicates fire?

B: IMPLEMENT fires on "implement"; DOCUMENT fires on "define"; API_CONTRACT fires on
"contract" in "data contracts". Three predicates → three routing decisions (4.1, 2.1, 3.3).
The model must not suppress API_CONTRACT because the user said "data contracts" rather
than "API contracts" — the term "contract" is in the API_CONTRACT lexicon regardless of modifier.

---

## Pair B15B1-WDA-teach-002 — "model" fires ARCHITECTURE

```
source_id: xiigen-b15-teach-002
pair_kind: teaching_unit
```

A: "Implement the domain model for the order service." Does ARCHITECTURE fire?

B: Yes. "model" is in the ARCHITECTURE lexicon. IMPLEMENT fires on "implement" and
ARCHITECTURE fires on "model". Two predicates → two routing decisions (4.1 for IMPLEMENT, 3.1 for ARCHITECTURE).
"Domain model" is an architectural concept — the lexicon correctly fires ARCHITECTURE even
though the developer's intent is to write implementation code.

---

## Pair B15B1-WDA-teach-003 — four-predicate routing example: all decisions are independent

```
source_id: xiigen-b15-teach-003
pair_kind: teaching_unit
```

A: "write the order service: implement the domain model, define the data contracts, code the business rules."
How many routing decisions does the router produce?

B: Four predicates fire (IMPLEMENT, DOCUMENT, API_CONTRACT, ARCHITECTURE). The router
produces FOUR independent routing decisions: 4.1 (IMPLEMENT), 2.1 (DOCUMENT), 3.3
(API_CONTRACT), 3.1 (ARCHITECTURE). No predicate is treated as dominant — each produces
its own decision. DecisionCount: 4.

---

## Pair B15B1-WDA-teach-004 — "scaffold" and "bootstrap" are in IMPLEMENT lexicon

```
source_id: xiigen-b15-teach-004
pair_kind: teaching_unit
```

A: "Scaffold the project and bootstrap the database connection." Which predicate fires?

B: IMPLEMENT fires on "scaffold" and "bootstrap". Both are code-generation/initialization
actions in the IMPLEMENT lexicon. The result is a single-predicate compile: CompiledPredicateIds:
["IMPLEMENT"]. "project" and "database" and "connection" do not independently fire predicates here.

---

## Pair B15B1-WDA-teach-005 — probationary_accepted vs probationary_rejected: the Gate B decision

```
source_id: xiigen-b15-teach-005
pair_kind: teaching_unit
```

A: "What is the difference between probationary_accepted and probationary_rejected as factory trust states?"

B:
  probationary_accepted: "The candidate flow passed Gate B (all 9 nodes covered, min confidence
  met) but has not yet accumulated enough production evidence pairs to be promoted to
  trusted_learned. The flow is SELECTED — the model uses it. The factory emits a warning
  noting the probationary status and progress toward promotion threshold."

  probationary_rejected: "The candidate flow failed Gate B — at least one node is not covered,
  or minimum confidence is below threshold. The flow is NOT selected. The fallback
  (universal-nine-node-development-flow) is used instead. RejectedCandidateFlowId records
  which candidate was rejected."

  Key distinction: "probationary_accepted → flow executes. probationary_rejected → fallback executes."
  Gate B is a binary gate — it does not partially accept a flow with caveats.
