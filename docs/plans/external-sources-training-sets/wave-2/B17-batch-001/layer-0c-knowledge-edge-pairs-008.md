# Layer 0-C — Knowledge-Edge Pairs (008)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: knowledge-edge

---

## B17-0C-008-001 — TransitionType determination in B17 context

**Rule 15 — TransitionType hierarchy (highest to lowest):**
1. refactoring (refactor verb present)
2. feature_addition (IMPLEMENT present, no refactor)
3. documentation (DOCUMENT or API_CONTRACT alone, no IMPLEMENT)
4. no contribution (QA or TEST alone, no IMPLEMENT)
5. none (no predicates fire)

**Determination algorithm:**
1. Check for refactor verb (refactor, restructure, rewrite) → if yes, refactoring
2. Check for IMPLEMENT predicate (no refactor) → if yes, feature_addition
3. Check for DOCUMENT predicate (no IMPLEMENT) → if yes, documentation
4. Check for API_CONTRACT alone (no IMPLEMENT) → documentation
5. Check for QA or TEST alone (no IMPLEMENT, no DOCUMENT) → no contribution
6. Otherwise → none

---

## B17-0C-008-002 — TransitionType examples for B17

| Input | Predicates | TransitionType | Reason |
|---|---|---|---|
| "Refactor the Stripe connector" | [IMPLEMENT, API_CONTRACT] | refactoring | refactor verb present |
| "Build the PayPal connector" | [IMPLEMENT, API_CONTRACT] | feature_addition | IMPLEMENT, no refactor |
| "Integrate the Slack API" | [API_CONTRACT] | documentation | API_CONTRACT alone |
| "Document the Plaid interface" | [DOCUMENT] | documentation | DOCUMENT alone |
| "QA the signing flow" | [QA] | no contribution | QA alone |
| "Write tests for the connector" | [TEST] | no contribution | TEST alone |

---

## B17-0C-008-003 — Refactor verb overrides all other TransitionType signals

**Rule 15 is absolute:** Any refactor verb present → TransitionType: refactoring, regardless of other verbs.

**Examples:**
- "Refactor and build the connector" → refactoring (refactor + build → refactoring wins)
- "Refactor and document the connector interface" → refactoring (refactor + document → refactoring wins)
- "Refactor, build, and write tests" → refactoring

**The refactoring determination happens at verb detection, before any other TransitionType consideration.**

---

## B17-0C-008-004 — IMPLEMENT alone (no refactor, no API_CONTRACT) → feature_addition

**Scenario:** "Implement the PaymentCalculatorService."
**Predicates:** ["IMPLEMENT"]
**SubPhase:** 4.1 (no integration vocabulary)
**TransitionType:** feature_addition

This is B17-adjacent but not B17-core — it lacks integration vocabulary. The TransitionType is still feature_addition because IMPLEMENT is present and there is no refactor verb.

**TransitionType is not affected by sub-phase routing.** Whether IMPLEMENT goes to 4.1 or 4.3, the TransitionType is feature_addition.
