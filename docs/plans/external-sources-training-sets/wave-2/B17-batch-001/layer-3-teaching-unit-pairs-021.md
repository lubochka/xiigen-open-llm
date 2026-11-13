# Layer 3 — Teaching Unit Pairs (021)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: teaching-unit | focus: Square POS connector

---

## B17-TU-021-001 — "POS" acronym does not contribute integration vocabulary

**A:** "Build the Square POS connector." Does "POS" contribute integration vocabulary to make IMPLEMENT route to 4.3?

**B:** No. "POS" (Point of Sale) is a product category acronym with no predicate. IMPLEMENT routes to 4.3 because "connector" is present — not because "POS" is present. If the request were "Build the Square POS service" (no "connector"), IMPLEMENT would route to 4.1.

**Correct source of 4.3 routing:** "connector" (Rule 4, integration vocabulary)
**Incorrect source (v6 error):** "POS" or "Square" or "payment" (proper nouns/domain nouns, no predicate)

---

## B17-TU-021-002 — "QA" abbreviation vs "quality assurance" written out

**A:** Input A: "Create the Square POS connector and run a QA pass." Input B: "Create the Square POS connector and perform quality assurance." Which input fires the QA predicate?

**B:** Input A only.

**Rule 21 (absolute):** QA fires on the abbreviation "QA" only. "quality assurance" written out does NOT fire the QA predicate. This is a strict lexical match.

**Input A result:** ["IMPLEMENT", "API_CONTRACT", "QA"] → 4.3, 3.3, 7.x
**Input B result:** ["IMPLEMENT", "API_CONTRACT"] → 4.3, 3.3

**Teaching point:** Rule 21 is one of the strictest rules in B17. Unlike most predicates where semantic synonyms are considered (e.g., "authenticate" fires SECURITY just like "authentication"), QA fires only on the exact abbreviation "QA". Written-out forms are invisible to the QA predicate.

---

## B17-TU-021-003 — Square OAuth merchant authorization → SECURITY + AUTHORIZATION

**A:** "Build the Square payment connector, implement the OAuth 2.0 merchant authorization flow, and validate the access token." What fires?

**B:**
- "Build", "implement" → IMPLEMENT
- "connector" → API_CONTRACT
- "OAuth" → AUTHORIZATION (Rule 7)
- "token" → SECURITY (Rule 6)
- "validate" + token context → SECURITY (Rule 8; reinforces)
- "merchant authorization" — "authorization" English word → NO predicate (not "OAuth")

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

**Teaching point:** In "OAuth 2.0 merchant authorization flow", AUTHORIZATION fires from "OAuth 2.0", not from "authorization". The English word "authorization" in "merchant authorization" does not fire the AUTHORIZATION predicate.

---

## B17-TU-021-004 — QAGatingFlow73 must be last in orchestration

**A:** If the Square POS connector request includes QA: "Create the Square POS connector, write unit tests for payment and refund handlers, and run a QA pass." What is the orchestration plan?

**B:**
```
Step 1: IntegrationConnectorFlow43 (4.3) — build SquarePosConnector
Step 2: APIContractFlow33 (3.3) — API contract
Step 3: TestingFlow51 (5.1) — tests (after Step 1 — code dependency)
Step 4: QAGatingFlow73 (7.x) — QA gate (absolute last — gates all prior flows)
```

QAGatingFlow73 cannot run in parallel with any other flow. It gates the complete pipeline output. Even if there are only two prior flows, QA is always last.
