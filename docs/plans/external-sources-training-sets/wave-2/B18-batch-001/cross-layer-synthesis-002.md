# Cross-Layer Synthesis (002)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: cross-layer-synthesis

## B18-CLS-002 — Full Seven-Layer Trace: OrderServiceRefactoring

**Request:** "Refactor the OrderService to extract the PaymentProcessor and ShippingCalculator into separate domain services."

---

### Layer 0-A: Compiler

**Input tokens:** refactor, OrderService, extract, PaymentProcessor, ShippingCalculator, domain services

**Predicate firing:**
- "refactor" → IMPLEMENT fires (Rule 1)
- "refactor" → TransitionType: refactoring (Rule 9, absolute — overrides all other signals)
- "extract" → IMPLEMENT reinforcement (refactoring vocabulary)
- No "structure", "architecture", "pattern", "design" → ARCHITECTURE does NOT fire
- No test/document/security vocabulary → TEST, DOCUMENT, SECURITY do not fire
- "domain services" → 4.1 vocabulary (NOT an ARCHITECTURE trigger)

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- DomainId: software-development
- GapHandler: none

**Note:** "domain services" is a code architecture concept used as a vocabulary label, not the trigger word "architecture". The token "architecture" does not appear → ARCHITECTURE absent.

---

### Layer 0-B: Router

**Input:** predicates ["IMPLEMENT"], TransitionType: refactoring, vocabulary: OrderService, extract, domain services

**Routing decisions:**
- IMPLEMENT + refactoring vocabulary → BackendServiceFlow41 (4.1)
- "OrderService", "PaymentProcessor", "ShippingCalculator", "domain services" → 4.1 service vocabulary
- No data vocabulary → DataMigrationFlow44 does NOT activate
- TransitionType: refactoring → BackendServiceFlow41 (not DataMigrationFlow44)

**Output:**
- IMPLEMENT → BackendServiceFlow41 (sub-phase 4.1)
- No secondary flows
- Single-flow execution

---

### Layer 0-C: Knowledge Edge

**Active edges for this request:**
- "refactor" → TransitionType: refactoring (Rule 9, absolute)
- "extract" → IMPLEMENT + refactoring (code target = 4.1)
- "OrderService" → 4.1 service vocabulary
- "domain services" → 4.1 vocabulary; "services" is not "architecture" token

**Key disambiguation: "refactor" + "extract" both present**
- Both are refactoring verbs → TransitionType: refactoring confirmed from two independent signals
- "extract" targets a code class (PaymentProcessor) → code artifact → 4.1 (not 4.4)
- Contrast: "extract" targeting data records → would route to 4.4

**No ARCHITECTURE edge active:** "domain services" is a noun phrase used in the code context, not the token "architecture" or "pattern" or "design" or "structure". Token-level matching applies.

---

### Layer 0-D: Flow Factory

**Factory mode determination:**
- OrderServiceRefactoring — scenario registered in trusted_learned (known refactoring pattern)
- Factory mode: trusted_learned
- Gate B type: test_regression_check (BackendServiceFlow41 standard gate)

**Factory output:**
```
FlowFactoryEntry {
  ScenarioId: "OrderServiceRefactoring",
  Flows: [BackendServiceFlow41],
  TransitionType: refactoring,
  FactoryMode: trusted_learned,
  GateB: {
    Type: test_regression_check,
    Condition: "all_pre_refactor_tests_pass_post_refactor"
  }
}
```

---

### Layer 1: Transition Orchestrator

**Ordering input:** [BackendServiceFlow41]

**Orchestration rules applied:**
- Single flow — no ordering dependencies to resolve
- TransitionType: refactoring — no new artifacts; existing tests must pass post-refactor
- No ARCHITECTURE → no ArchitectureFlow31 prerequisite
- No TEST predicate → UnitTestingFlow51 not scheduled (regression is a Gate B check, not a TEST flow)

**Execution schedule:**
1. BackendServiceFlow41 — refactor OrderService; extract PaymentProcessor and ShippingCalculator

**Gate B check (N07/N08):** all pre-existing OrderService tests pass after extraction.

---

### Layer 2: Nine-Node Trace (BackendServiceFlow41)

**N01 — Input:** "refactor OrderService; extract PaymentProcessor and ShippingCalculator"

**N02 — Scope:** OrderService.cs; target extractions: PaymentProcessor.cs, ShippingCalculator.cs; domain services layer

**N03 — Analysis:** dependency mapping; identify OrderService methods belonging to payment vs shipping; assess coupling

**N04 — Specification:**
- PaymentProcessor.cs (extracted from OrderService)
- ShippingCalculator.cs (extracted from OrderService)
- OrderService.cs (modified — delegates to extracted services)
- IPaymentProcessor.cs (interface for fabric-first pattern)
- IShippingCalculator.cs (interface for fabric-first pattern)

**N05 — Generation:** extract PaymentProcessor and ShippingCalculator; update OrderService to inject via interfaces

**N06 — Execution:** compile; run all OrderService unit tests; run integration tests

**N07 — Pre-Gate:** regression check: all tests that passed before refactoring still pass after

**N08 — Gate B:**
- Type: test_regression_check
- Condition: `all_pre_refactor_tests_pass_post_refactor == true`
- Status: PASS (no test regressions)

**N09 — Output:** OrderService refactored; PaymentProcessor and ShippingCalculator extracted as separate domain services; Gate B passed.

---

### Layer 3: Teaching Point

**Core lesson from OrderServiceRefactoring:**

Rule 9 is absolute: "refactor" sets TransitionType: refactoring regardless of what else appears in the request. Even if the result creates new classes (PaymentProcessor, ShippingCalculator), the verb "refactor" dominates.

**Test question:** "Refactor the OrderService and write a migration to move historical orders to the archive table" → what is TransitionType?
- Answer: refactoring. "refactor" is present → Rule 9 fires → TransitionType: refactoring, even though migration is also present.
- Sub-phases: 4.1 (refactoring) + 4.4 (migration). Both sub-phases fire.

**Test question:** "Extract the PaymentProcessor into a new service class" → TransitionType?
- Answer: refactoring. "extract" (code target) is in the refactoring vocabulary.

---

### Cross-Layer Invariants Verified

| Layer | Input | Output | Consistent? |
|---|---|---|---|
| 0-A Compiler | "refactor" + "extract" | ["IMPLEMENT"], refactoring | YES |
| 0-B Router | ["IMPLEMENT"], refactoring | 4.1, BackendServiceFlow41 | YES |
| 0-C Knowledge | "refactor"/"extract" code target | refactoring edge, 4.1 routing | YES |
| 0-D Factory | OrderServiceRefactoring | trusted_learned, test_regression_check | YES |
| L1 Orchestrator | [BackendServiceFlow41] | single flow, regression Gate B | YES |
| L2 Nine-Node | BackendServiceFlow41 | Gate B: test_regression_check PASS | YES |
| L3 Teaching | "refactor" presence | Rule 9: TransitionType always refactoring | YES |
