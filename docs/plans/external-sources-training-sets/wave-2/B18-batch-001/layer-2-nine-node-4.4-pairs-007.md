# Layer 2 — Nine-Node 4.4 Pairs (007)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: nine-node-4.4

## B18-N9-007-001 — OrderServiceRefactoring: N01 input (BackendServiceFlow41)

**Scenario:** Refactor OrderService — extract PaymentProcessor and ShippingCalculator.

**N01 input:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- ConnectorName: OrderServiceRefactoring
- DomainId: "software-development"
- Flow: BackendServiceFlow41
- Sub-phase: 4.1

**Note:** This is a REFACTORING scenario. BackendServiceFlow41 (4.1) is used. The nine-node flow structure applies to BackendServiceFlow41 as well.

---

## B18-N9-007-002 — N02-N03: refactoring scope and code analysis

**N02 — Scope Definition:**
- Target: Extract PaymentProcessor and ShippingCalculator from OrderService
- OrderService currently: 450 lines; handles order placement, payment, shipping, notification
- Post-refactoring: OrderService (~150 lines); PaymentProcessor (~120 lines); ShippingCalculator (~100 lines)
- Public interface: OrderService.PlaceOrder() must remain unchanged (no breaking change)

**N03 — Existing Code Analysis:**
- OrderService.ProcessPayment() → 80 lines; standalone logic; no shared state with other OrderService methods
- OrderService.CalculateShipping() → 60 lines; depends on ShippingRateTable lookup; no shared state
- OrderService has 12 methods; 2 are PaymentProcessor candidates; 2 are ShippingCalculator candidates

---

## B18-N9-007-003 — N04-N05: refactoring plan and code generation

**N04 — Refactoring Plan:**
- Create IPaymentProcessor interface (ProcessPayment, ValidatePaymentMethod)
- Create PaymentProcessor implementing IPaymentProcessor
- Create IShippingCalculator interface (CalculateShipping, GetShippingRates)
- Create ShippingCalculator implementing IShippingCalculator
- Update OrderService: inject IPaymentProcessor and IShippingCalculator via constructor
- Remove extracted methods from OrderService

**N05 — Code Generation:**
1. IPaymentProcessor.cs (interface)
2. PaymentProcessor.cs (extracted implementation)
3. IShippingCalculator.cs (interface)
4. ShippingCalculator.cs (extracted implementation)
5. OrderService.cs (refactored — delegates to injected services)
6. OrderServiceTests_updated.cs (update DI setup in tests)

Wait — test updates are part of the refactoring (callers updated). 6 files but some are updates to existing files.

**N05 actual file count:** 3 new + 3 updated = 6 file operations, but "files generated" = 3 new files.

---

## B18-N9-007-004 — N06-N07: BackendServiceFlow41 execution and regression tests

**N06 — Execution (code transformation):**
1. Create IPaymentProcessor.cs and PaymentProcessor.cs
2. Create IShippingCalculator.cs and ShippingCalculator.cs
3. Refactor OrderService.cs: remove payment/shipping methods; add constructor injection
4. Update all test files that mock OrderService (update DI setup)
5. Compile check: ensure no compilation errors

**N07 — Regression Test Run:**
- Run all existing OrderService tests
- Run all PaymentService tests
- Run all ShippingService tests
- Expected: all pass (refactoring should not change behavior)
- If any fail: rollback all changes; log failure reason

---

## B18-N9-007-005 — N08 and N09: OrderServiceRefactoring

**N08 Gate B:**
- count(passing_tests_after) >= count(passing_tests_before) ✓ (same tests pass)
- OrderService compiles without errors ✓
- PaymentProcessor.cs compiles without errors ✓
- ShippingCalculator.cs compiles without errors ✓
- OrderService.PlaceOrder() public interface unchanged ✓

**N08 verdict: PASS**

**N09 output:**
```
{
  "connector_name": "OrderServiceRefactoring",
  "flow": "BackendServiceFlow41",
  "sub_phase": "4.1",
  "transition_type": "refactoring",
  "gate_b_status": "PASS",
  "gate_b_condition": "count(passing_tests_after) >= count(passing_tests_before) AND compilation clean",
  "files_generated": 3,
  "artifacts": [
    "PaymentProcessor.cs",
    "ShippingCalculator.cs",
    "OrderService_refactored.cs"
  ],
  "n08_verdict": "PASS",
  "completion_message": "OrderService refactored; PaymentProcessor + ShippingCalculator extracted; 3 files"
}
```

---

## B18-N9-007-006 — Refactoring nine-node vs migration nine-node: comparison

| Node | DataMigrationFlow44 (migration) | BackendServiceFlow41 (refactoring) |
|---|---|---|
| N01 | feature_addition, 4.4 | refactoring, 4.1 |
| N02 | Source→target data scope | Code scope (which classes to refactor) |
| N03 | Source data analysis | Existing code analysis (method scan) |
| N04 | Target schema specification | Refactoring plan (new interfaces, extraction plan) |
| N05 | SQL script generation | Code file generation |
| N06 | Batch data execution | Code transformation execution |
| N07 | Row count pre-verification | Regression test run |
| N08 | Row count assertion Gate B | Test regression Gate B |
| N09 | "Migration complete; N files" | "Refactoring complete; N files" |
