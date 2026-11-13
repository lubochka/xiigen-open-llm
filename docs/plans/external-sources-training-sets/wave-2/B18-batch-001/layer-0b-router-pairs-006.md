# Layer 0-B — Router Pairs (006)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: router

## B18-0B-006-001 — [IMPLEMENT] rename/restructure → 4.1 (service vocabulary, refactoring)

**Scenario:** PaymentServiceRefactoring (rename + restructure)

**Router input:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Vocabulary: "rename", "restructure", "methods", "routing" → 4.1 signals
- Request: "Rename the CustomerController to AccountController and restructure the routing methods to match the new API spec."

**Routing logic:**
1. Predicate: IMPLEMENT
2. TransitionType: refactoring (rename + restructure vocabulary)
3. Vocabulary: "controller", "methods", "routing" → 4.1 (service/method vocabulary)
4. "API spec" does NOT fire 4.3 — it is a reference target, not an integration task
5. Sub-phase: 4.1

**Router output:**
- Flow: BackendServiceFlow41
- Sub-phase: 4.1
- TransitionType: refactoring

---

## B18-0B-006-002 — "rename" alone → BackendServiceFlow41, 4.1

**Request:** "Rename the PaymentGatewayService to StripePaymentAdapter."

**Router input:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Vocabulary: "rename", "service" → 4.1

**Router output:**
- Flow: BackendServiceFlow41, sub-phase 4.1

---

## B18-0B-006-003 — "restructure" alone → BackendServiceFlow41, 4.1

**Request:** "Restructure the OrderService to separate read and write operations."

**Router input:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Vocabulary: "restructure", "service", "operations" → 4.1

**Router output:**
- Flow: BackendServiceFlow41, sub-phase 4.1

---

## B18-0B-006-004 — rename + consolidate: still 4.1, still refactoring

**Request:** "Rename the three PaymentMethod subclasses and consolidate their shared validation logic into the base class."

**Router input:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Vocabulary: "rename", "consolidate", "class" (x3), "validation", "base class" → 4.1

**Router output:**
- Flow: BackendServiceFlow41, sub-phase 4.1, TransitionType: refactoring

---

## B18-0B-006-005 — "API spec" in rename context — 4.3 does NOT fire

**Teaching case:**
Request: "Rename all endpoint methods to match the OpenAPI specification."

**Router input:**
- predicates: ["IMPLEMENT"] (rename fires IMPLEMENT; no API_CONTRACT predicate)
- TransitionType: refactoring
- "OpenAPI specification" → is this 4.3? No. Vocabulary signals 4.3 are: connector, webhook, integration, external service. "OpenAPI specification" as a reference target for renaming does NOT fire 4.3.
- Vocabulary: "rename", "endpoint", "methods" → 4.1

**Router output:**
- Flow: BackendServiceFlow41, sub-phase 4.1

**Why not 4.3:** The task is renaming code methods (4.1 refactoring). The OpenAPI spec is the standard being followed, not an integration being built.

---

## B18-0B-006-006 — refactoring routing summary: 4.1 vocabulary table

**Complete 4.1 routing vocabulary for B18:**

| Word/phrase | Routing signal strength |
|---|---|
| service | Strong 4.1 signal |
| class, classes | Strong 4.1 signal |
| method, methods | Strong 4.1 signal |
| function, functions | Strong 4.1 signal |
| module, modules | Strong 4.1 signal |
| repository, repositories | Strong 4.1 signal |
| domain | Strong 4.1 signal |
| rule, rules (business) | Strong 4.1 signal |
| business logic | Strong 4.1 signal |
| handler, handlers | Strong 4.1 signal |
| controller | Strong 4.1 signal (service layer) |
| layer | Moderate 4.1 signal |
| interface | Moderate 4.1 signal |
| base class | Moderate 4.1 signal |
| dependency | Moderate 4.1 signal |

**None of these signal 4.4:** No data migration, no seeding, no ETL context.
