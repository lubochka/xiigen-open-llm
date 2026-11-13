# Layer 0-A — Compiler Pairs (023)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-023-001 — TaxService: "implement" → IMPLEMENT

A: "Implement the tax calculation service that applies jurisdiction-specific tax rates to order line items."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Sub-phase: 4.1
Evidence: implement → IMPLEMENT; tax calculation service → backend service noun.

---

## B15-compiler-023-002 — TaxService: IMPLEMENT+ARCHITECTURE combo — "tax model"

A: "Build the tax service and design the domain model for tax jurisdiction rules and exemption categories."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; "domain model" → ARCHITECTURE fires. Tax jurisdiction model is an architectural domain model — ARCHITECTURE fires regardless of domain specificity.

---

## B15-compiler-023-003 — TaxService: IMPLEMENT+ARCHITECTURE — "architecture" keyword explicit

A: "Architect the tax calculation engine and implement the rate lookup and exemption logic."
B: Predicates: ["ARCHITECTURE", "IMPLEMENT"] | TransitionType: feature_addition
Evidence: architect + "architecture" → ARCHITECTURE; implement → IMPLEMENT. Both fire independently.

---

## B15-compiler-023-004 — TaxService: four predicates — architect + define contracts + implement + document

A: "Architect the tax service domain model, define the tax API contracts, implement the calculation engine, and document the tax rules."
B: Predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "DOCUMENT"] | TransitionType: feature_addition
Evidence: architect + domain model → ARCHITECTURE; "API contracts" → API_CONTRACT; implement → IMPLEMENT; document → DOCUMENT. Four predicates, four routing decisions.

---

## B15-compiler-023-005 — TaxService: v6-02 corruption — "tax architecture" not caught

v6 corrupt: "Architect the tax service domain model and implement the calculation engine" → Predicates: ["IMPLEMENT"]
Correct: Predicates: ["ARCHITECTURE", "IMPLEMENT"]
Error type: v6-02. "architect" + "domain model" fires ARCHITECTURE. The corrupt model treated "tax service domain model" as domain jargon and suppressed ARCHITECTURE. Both "architect" (verb) and "domain model" (noun phrase) independently trigger ARCHITECTURE.
