# Layer 0-A — Compiler Pairs (008)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: compiler

---

## B17-0A-008-001 — Refactoring TransitionType: IMPLEMENT + API_CONTRACT

**Input:**
"Refactor the legacy payment gateway connector to use the new Stripe SDK and implement the updated charge flow."

**Compiler analysis:**
- "Refactor" → IMPLEMENT (Rule 14: "refactor" fires IMPLEMENT)
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT (Rule 4)
- Integration vocabulary: "connector", "gateway", "SDK" → IMPLEMENT routes to 4.3
- Refactor verb present → TransitionType: refactoring (Rule 14, overrides feature_addition)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**SubPhase:** 4.3 (IMPLEMENT), 3.3 (API_CONTRACT)
**TransitionType:** refactoring (Rule 15: refactoring > feature_addition in hierarchy)
**DomainId:** software-development

---

## B17-0A-008-002 — "refactor" alone → IMPLEMENT + refactoring TransitionType

**Input:**
"Refactor the StripeConnector to use the v3 API client."

**Compiler analysis:**
- "Refactor" → IMPLEMENT
- "connector" → API_CONTRACT
- Integration vocabulary → 4.3
- TransitionType: refactoring

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**SubPhase:** 4.3, 3.3
**TransitionType:** refactoring
**DomainId:** software-development

---

## B17-0A-008-003 — Refactoring TransitionType hierarchy: refactoring > feature_addition

**Scenario:**
Task A: "Build the Stripe connector." → TransitionType: feature_addition
Task B: "Refactor the Stripe connector." → TransitionType: refactoring
Task C: "Refactor and build the Stripe connector." → TransitionType: refactoring

**Rule:** TransitionType hierarchy is: refactoring > feature_addition > documentation > no contribution > none
When "refactor" is present alongside "build"/"implement", refactoring wins (Rule 15).

**Teaching point:** The presence of any refactor verb ("refactor", "restructure", "rewrite") elevates TransitionType to refactoring regardless of other build verbs present.

---

## B17-0A-008-004 — Refactor + DOCUMENT → refactoring TransitionType (not documentation)

**Input:**
"Refactor the legacy PayPal connector and document the new interface."

**Compiler analysis:**
- "Refactor" → IMPLEMENT → TransitionType: refactoring
- "connector" → API_CONTRACT
- "document" → DOCUMENT
- TransitionType: refactoring (refactoring > documentation in hierarchy — Rule 15)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
**SubPhase:** 4.3, 3.3, 2.1
**TransitionType:** refactoring (hierarchy: refactoring wins over documentation)
**DomainId:** software-development

**Teaching point:** The TransitionType is determined by the highest-priority verb in the hierarchy. Even when DOCUMENT is present alongside refactor, TransitionType remains refactoring.
