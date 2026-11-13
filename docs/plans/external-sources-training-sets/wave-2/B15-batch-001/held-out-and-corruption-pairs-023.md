# Held-Out and Corruption Pairs (023)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: held_out_corruption

## HC-023-A — Held-out: TaxService six predicates

A: "Design tax domain model, define tax API contracts, build tax service, secure tax data endpoints, write tax calculation tests, document the tax rules."
B: Predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT"] | feature_addition | DecisionCount: 6

## HC-023-B — Held-out: "architect" verb fires ARCHITECTURE

A: "Architect the tax calculation engine and implement the rate lookup."
B: Predicates: ["ARCHITECTURE", "IMPLEMENT"] | "architect" (verb) fires ARCHITECTURE.

## HC-023-C — Held-out: "tax model" fires ARCHITECTURE

A: "Build the tax service and define the tax model for multi-jurisdiction compliance."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE", "DOCUMENT"] | "tax model" fires ARCHITECTURE on "model"; "define" fires DOCUMENT. Three predicates.

## HC-023-D — Corruption v6-02: "architect" not caught as ARCHITECTURE

v6 corrupt: "Architect the tax calculation engine and implement the rate lookup" → Predicates: ["IMPLEMENT"]
Correct: Predicates: ["ARCHITECTURE", "IMPLEMENT"] | "architect" (verb) fires ARCHITECTURE.
Error: v6-02. The corrupt model treated "architect" as a job title noun and suppressed ARCHITECTURE. "architect" as a verb ("architect the engine") is a first-class ARCHITECTURE trigger.

## HC-023-E — Corruption v6-02: "tax model" not caught

v6 corrupt: "Build the tax service and define the tax model" → Predicates: ["IMPLEMENT", "DOCUMENT"]
Correct: Predicates: ["IMPLEMENT", "ARCHITECTURE", "DOCUMENT"] | "tax model" → ARCHITECTURE fires on "model".
Error: v6-02. Any "[X] model" phrase fires ARCHITECTURE when X is a domain noun.
