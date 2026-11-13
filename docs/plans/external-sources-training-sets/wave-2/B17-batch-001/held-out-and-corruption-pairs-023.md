# Held-Out and Corruption Pairs (023)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption | focus: Multi-connector orchestration

---

## B17-HO-023-001 — HELD-OUT: "build" + multi-connector → same predicates as single connector

**Input:**
"Build the Stripe and Avalara connector pair — implement the checkout payment connector and the tax calculation connector as a coordinated integration."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3, 3.3
TransitionType: feature_addition
DecisionCount: 2

**Gate B:** Predicate set ["IMPLEMENT", "API_CONTRACT"] — same as single connector. No additional predicates from naming two connectors.

---

## B17-HO-023-002 — HELD-OUT: "integrate" multi-connector → API_CONTRACT only

**Input:**
"Integrate the Intercom, Mailgun, and Square connectors with the central CRM orchestration layer."

**Expected output:**
CompiledPredicateIds: ["API_CONTRACT"]
SubPhase: 3.3
TransitionType: documentation
DecisionCount: 1

**Gate B:** IMPLEMENT absent. "integrate" + multi-connector context → API_CONTRACT only. DecisionCount: 1.

---

## B17-HO-023-003 — CORRUPTION: multi-connector request → IMPLEMENT invented from "integrate" (v6-01)

**Input:**
"Integrate five payment connectors into the checkout pipeline."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3, 3.3

**CORRECT output:**
CompiledPredicateIds: ["API_CONTRACT"]
SubPhase: 3.3

**Error:** IMPLEMENT invented because "five connectors" and "checkout pipeline" seem to imply building code. Rule 1 is unconditional and context-independent. "integrate" → API_CONTRACT only, regardless of scale or context.

---

## B17-HO-023-004 — CORRUPTION: predicates multiplied per connector count

**Input:**
"Build the Stripe connector, the Avalara connector, and the Zuora connector."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "API_CONTRACT", "API_CONTRACT"]
OR
DecisionCount: 6 (treating each connector as separate predicate set)

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
DecisionCount: 2

**Error:** Predicates multiplied by connector count. CompiledPredicateIds is a deduplicated set. Three connector mentions → API_CONTRACT fires once. DecisionCount is 2 (one per predicate), not 6.
