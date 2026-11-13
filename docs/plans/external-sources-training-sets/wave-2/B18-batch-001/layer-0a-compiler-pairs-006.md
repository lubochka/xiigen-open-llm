# Layer 0-A — Compiler Pairs (006)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: compiler

## B18-0A-006-001 — "rename" + "restructure" → IMPLEMENT; refactoring

**Request:** "Rename the CustomerController to AccountController and restructure the routing methods to match the new API spec."

**Compiler trace:**
- Token: "rename" → IMPLEMENT (refactoring vocabulary, Rule 19)
- Token: "restructure" → IMPLEMENT (refactoring vocabulary, Rule 1)
- Token: "methods" → 4.1 sub-phase signal (service/method vocabulary)
- Token: "API spec" → does API_CONTRACT fire? No. "spec" without explicit API_CONTRACT vocabulary does not fire alone. "rename" and "restructure" are dominant terms. IMPLEMENT only.
- TransitionType: refactoring ("rename" and "restructure" both fire refactoring TransitionType)

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.1

**Key rule for "API spec":** "API spec" in the context of renaming/restructuring does not trigger API_CONTRACT. The compiler looks for explicit API_CONTRACT vocabulary (define API, contract, OpenAPI, Swagger, API specification as primary task noun). Here "API spec" is the target reference, not the task.

---

## B18-0A-006-002 — "rename" alone → IMPLEMENT, refactoring

**Request:** "Rename the PaymentGatewayService to StripePaymentAdapter to reflect its specific implementation."

**Compiler trace:**
- Token: "rename" → IMPLEMENT (Rule 19 — rename fires IMPLEMENT, TransitionType: refactoring)
- Token: "service" → 4.1 sub-phase signal
- Token: "adapter" → 4.1 sub-phase signal (service vocabulary)
- TransitionType: refactoring

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.1

---

## B18-0A-006-003 — "restructure" alone → IMPLEMENT, refactoring

**Request:** "Restructure the OrderService to separate read operations from write operations."

**Compiler trace:**
- Token: "restructure" → IMPLEMENT (Rule 1 — restructure fires IMPLEMENT, refactoring)
- Token: "service" → 4.1 sub-phase signal
- Token: "operations" → 4.1 sub-phase signal
- TransitionType: refactoring

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.1

---

## B18-0A-006-004 — "rename" + "consolidate" → IMPLEMENT, refactoring

**Request:** "Rename the three PaymentMethod subclasses and consolidate their shared validation logic into the base class."

**Compiler trace:**
- Token: "rename" → IMPLEMENT (refactoring)
- Token: "consolidate" → IMPLEMENT (refactoring, Rule 20)
- Token: "class" → 4.1 sub-phase signal
- Token: "validation" → 4.1 sub-phase signal (domain/rule vocabulary)
- Token: "base class" → 4.1 sub-phase signal
- TransitionType: refactoring (both rename and consolidate fire refactoring; refactoring is still refactoring when stacked)

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.1

---

## B18-0A-006-005 — PaymentServiceRefactoring: rename + consolidate full trace

**Request:** "Refactor the PaymentService: rename processPayment() to executeTransaction(), rename validateCard() to verifyPaymentMethod(), and consolidate the three validation helper methods into a single validateAll() method."

**Compiler trace:**
- Token: "refactor" → IMPLEMENT (Rule 1, refactoring)
- Token: "rename" (x2) → IMPLEMENT (Rule 19, refactoring)
- Token: "consolidate" → IMPLEMENT (Rule 20, refactoring)
- Token: "service" → 4.1 sub-phase signal
- Token: "methods" → 4.1 sub-phase signal
- Token: "validation" → 4.1 sub-phase signal
- TransitionType: refactoring (dominant verbs: refactor, rename, consolidate — all refactoring)

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.1
ConnectorName: PaymentServiceRefactoring

---

## B18-0A-006-006 — "restructure" + "architecture" → IMPLEMENT+ARCHITECTURE, refactoring

**Request:** "Restructure the routing layer to follow the clean architecture pattern."

**Compiler trace:**
- Token: "restructure" → IMPLEMENT (refactoring)
- Token: "architecture" → ARCHITECTURE fires (Rule 7 — architecture fires ARCHITECTURE)
- Token: "pattern" → ARCHITECTURE fires (Rule 7)
- Token: "routing" → 4.1 sub-phase signal (service/layer vocabulary)
- TransitionType: refactoring ("restructure" is the primary action verb; refactoring > feature_addition)

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: refactoring, sub-phases: 4.1 + 3.1
