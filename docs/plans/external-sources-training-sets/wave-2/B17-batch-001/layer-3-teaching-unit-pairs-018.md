# Layer 3 — Teaching Unit Pairs (018)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: teaching-unit | focus: Zuora billing connector

---

## B17-TU-018-001 — "subscription lifecycle" without connector vocabulary → 4.1 not 4.3

**A:** "Implement the subscription renewal logic and billing cycle state machine." Does this route to 4.3?

**B:** No. "subscription renewal logic" and "billing cycle state machine" are internal implementation patterns — billing domain terms without integration vocabulary. IMPLEMENT routes to 4.1.

**Contrast:**
- "Implement the subscription renewal logic" → IMPLEMENT → 4.1 (no integration vocabulary)
- "Implement the Zuora subscription connector" → IMPLEMENT + API_CONTRACT → 4.3 (connector)

The billing domain context (subscription, renewal, billing cycle) does not override the routing rule. Only the integration vocabulary tokens change IMPLEMENT routing from 4.1 to 4.3.

---

## B17-TU-018-002 — "OAuth client credentials" → AUTHORIZATION fires from "OAuth"

**A:** "Build the Zuora connector, implement the OAuth 2.0 client credentials flow." What fires AUTHORIZATION?

**B:** The word "OAuth" fires AUTHORIZATION (Rule 7). "client credentials" is the OAuth grant type name — it is a descriptive phrase but does not independently fire any predicate. AUTHORIZATION fires from "OAuth", and SECURITY fires from "token" (if present in the same input).

"OAuth 2.0 client credentials flow" → AUTHORIZATION fires from "OAuth 2.0"
"client credentials flow" alone (without "OAuth") → NO AUTHORIZATION

---

## B17-TU-018-003 — "token renewal" fires SECURITY (Rule 6) — billing context irrelevant

**A:** In "Implement the Zuora connector with automatic credential token renewal", does SECURITY fire?

**B:** Yes. "token" fires SECURITY (Rule 6) unconditionally. "credential token renewal" contains the word "token". SECURITY fires regardless of whether the token is a billing credential, access token, or any other type.

**Result:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"] from "token renewal".

"Renewal" is a billing term — it fires no predicate. The SECURITY trigger is from "token" only.

---

## B17-TU-018-004 — "refactor Zuora connector" → refactoring TransitionType

**A:** "Refactor the Zuora billing connector to support Zuora API v1.3 schema changes." What is TransitionType?

**B:** TransitionType: refactoring.

"Refactor" → IMPLEMENT (Rule 14: refactor fires IMPLEMENT + sets TransitionType: refactoring)
"connector" → API_CONTRACT
"API" → API_CONTRACT (reinforces)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**TransitionType:** refactoring

The N08 arbiter will additionally check that the refactoring maintains backward compatibility in the connector interface (existing method signatures preserved).
