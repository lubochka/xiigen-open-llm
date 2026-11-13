# Layer 0-A — Compiler Pairs (009)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: compiler

## B18-0A-009-001 — IMPLEMENT+ARCHITECTURE: refactoring + pattern

**Request:** "Refactor the OrderRepository to follow the repository pattern architecture."

**Compiler trace:**
- Token: "refactor" → IMPLEMENT (refactoring vocabulary, Rule 1)
- Token: "repository" → 4.1 sub-phase signal (repository vocabulary)
- Token: "pattern" → ARCHITECTURE fires (Rule 7 — "pattern" fires ARCHITECTURE)
- Token: "architecture" → ARCHITECTURE fires (Rule 7 — "architecture" fires ARCHITECTURE)
- TransitionType: refactoring ("refactor" verb = refactoring; refactoring > feature_addition in hierarchy)

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: refactoring, sub-phases: 4.1 + 3.1

---

## B18-0A-009-002 — "clean architecture" fires ARCHITECTURE

**Request:** "Refactor the service layer to comply with clean architecture principles."

**Compiler trace:**
- Token: "refactor" → IMPLEMENT (refactoring)
- Token: "service layer" → 4.1 sub-phase signal
- Token: "architecture" → ARCHITECTURE fires (Rule 7)
- Token: "principles" → ARCHITECTURE reinforcement (design principles vocabulary)
- TransitionType: refactoring

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: refactoring, sub-phases: 4.1 + 3.1

---

## B18-0A-009-003 — "extract" + "pattern" → IMPLEMENT+ARCHITECTURE, refactoring

**Request:** "Extract the business logic from OrderController into a dedicated OrderDomainService following the domain service pattern."

**Compiler trace:**
- Token: "extract" → IMPLEMENT (refactoring vocabulary, Rule 1)
- Token: "business logic" → 4.1 sub-phase signal (business/domain vocabulary)
- Token: "service" → 4.1 sub-phase signal
- Token: "pattern" → ARCHITECTURE fires (Rule 7)
- Token: "domain service" → 4.1 sub-phase reinforcement (domain vocabulary)
- TransitionType: refactoring ("extract" is the primary verb)

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: refactoring, sub-phases: 4.1 + 3.1

---

## B18-0A-009-004 — refactoring + "pattern" + TEST

**Request:** "Refactor the UserRepository to follow the unit of work pattern and write tests to verify the refactored interface."

**Compiler trace:**
- Token: "refactor" → IMPLEMENT (refactoring)
- Token: "repository" → 4.1 sub-phase signal
- Token: "pattern" → ARCHITECTURE fires (Rule 7)
- Token: "write tests" → TEST (Rule 12)
- TransitionType: refactoring

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE", "TEST"], TransitionType: refactoring, sub-phases: 4.1 + 3.1 + 5.x

---

## B18-0A-009-005 — OrderServiceRefactoring: extract pattern

**Request:** "Refactor the OrderService by extracting the PaymentProcessor and ShippingCalculator into standalone services."

**Compiler trace:**
- Token: "refactor" → IMPLEMENT (refactoring)
- Token: "extract" → IMPLEMENT (refactoring vocabulary)
- Token: "service" (x3) → 4.1 sub-phase signal
- Token: "standalone services" → 4.1 sub-phase signal
- No ARCHITECTURE trigger: no "pattern", "structure", "architecture", "design" noun
- TransitionType: refactoring

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.1
ConnectorName: OrderServiceRefactoring

---

## B18-0A-009-006 — "follow the pattern" — "pattern" alone fires ARCHITECTURE

**Request:** "Reorganize the data access layer to follow the repository pattern."

**Compiler trace:**
- Token: "reorganize" → IMPLEMENT (refactoring vocabulary, Rule 1)
- Token: "data access layer" → 4.1 sub-phase signal (layer/service vocabulary)
- Token: "repository" → 4.1 sub-phase signal
- Token: "pattern" → ARCHITECTURE fires (Rule 7 — "pattern" always fires ARCHITECTURE)
- TransitionType: refactoring

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: refactoring, sub-phases: 4.1 + 3.1

**Teaching:** "pattern" fires ARCHITECTURE even in "follow the pattern" phrasing. The compiler is literal on this token.
