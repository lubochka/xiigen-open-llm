# Held-Out and Corruption Pairs (021)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption | focus: Square POS connector

---

## B17-HO-021-001 — HELD-OUT: "create" + "connector" + "QA" abbreviation

**Input:**
"Create the Square POS connector for in-store payments, write unit tests for the payment and refund handlers, and run a QA pass on the end-to-end checkout flow."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST", "QA"]
SubPhase: 4.3, 3.3, 5.1, 7.x
TransitionType: feature_addition

**Gate B:** QA fires from "QA" abbreviation. TEST fires from "write unit tests". IMPLEMENT → 4.3. QA is last in canonical order.

---

## B17-HO-021-002 — HELD-OUT: "quality assurance" written out → no QA predicate

**Input:**
"Create the Square POS connector and perform quality assurance on the checkout flow."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3, 3.3
TransitionType: feature_addition

**Gate B:** QA must NOT be present ("quality assurance" written out ≠ "QA" abbreviation). This is the held-out test for Rule 21.

---

## B17-HO-021-003 — CORRUPTION: "quality assurance" → QA invented (v6-01 variant, Rule 21)

**Input:**
"Build the Square payment connector and perform quality assurance testing."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "QA"]

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]

**Error:** QA invented from "quality assurance" (written out). Rule 21 is strict: only the abbreviation "QA" fires the QA predicate. "quality assurance" written out does NOT fire QA.

---

## B17-HO-021-004 — CORRUPTION: "POS" treated as integration vocabulary (v6-03 attribution)

**Input:**
"Implement the Square POS connector."

**CORRUPTED output (with wrong attribution):**
SubPhase: 4.3 attributed to "POS" being integration vocabulary

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3 (correctly attributed to "connector")

**Error:** The routing is correct (4.3) but the attribution is wrong. "POS" is a product category acronym with no predicate. The 4.3 routing comes from "connector" (Rule 4: integration vocabulary). If asked to explain the routing, the model must cite "connector" not "POS".
