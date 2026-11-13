# Layer 2 — Nine-Node 4.4 Pairs (008)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: nine-node-4.4

## B18-N9-008-001 — PaymentServiceRefactoring: N01 input

**Scenario:** Refactor PaymentService — rename methods, consolidate validation.

**N01 input:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- ConnectorName: PaymentServiceRefactoring
- DomainId: "software-development"
- Flow: BackendServiceFlow41
- Sub-phase: 4.1

---

## B18-N9-008-002 — N02-N03: scope and code analysis

**N02 — Scope Definition:**
- Rename: processPayment() → executeTransaction()
- Rename: validateCard() → verifyPaymentMethod()
- Consolidate: three validation helper methods (validateAmount, validateCurrency, validateCard) → validateAll()
- Scope: PaymentService.cs (320 lines); PaymentServiceTests.cs (185 lines)
- Post-refactoring: PaymentService.cs (~280 lines, cleaner); PaymentServiceTests.cs (updated method names)

**N03 — Code Analysis:**
- processPayment(): 45 lines; called from 6 locations (OrderService, CheckoutController, RefundController, AdminController, BillingService, WebhookHandler)
- validateCard(): 30 lines; called from 4 locations
- validateAmount(): 20 lines; validateCurrency(): 15 lines; validateCard() (helper): 25 lines
- All three helpers are private; consolidated into private validateAll()

---

## B18-N9-008-003 — N04-N05: refactoring plan and code generation

**N04 — Refactoring Plan:**
- Rename processPayment() → executeTransaction() in PaymentService.cs AND all 6 callers
- Rename validateCard() (public) → verifyPaymentMethod() in PaymentService.cs AND all 4 callers
- Replace validateAmount + validateCurrency + validateCard (private helper) with consolidated validateAll()
- Update PaymentServiceTests.cs: method name references
- Update all caller files: method name references

**N05 — Files to generate/update:**
1. PaymentService_refactored.cs (renamed + consolidated)
2. PaymentServiceTests_updated.cs (method names updated)
3. caller_updates.diff (shows all 10 caller location changes)
4. refactoring_summary.md (documents old→new method name mapping)

---

## B18-N9-008-004 — N06-N07: execution and regression

**N06 — Execution:**
1. Rename processPayment → executeTransaction in PaymentService.cs
2. Update all 6 callers: search for processPayment() → replace with executeTransaction()
3. Rename validateCard → verifyPaymentMethod in PaymentService.cs
4. Update all 4 callers: search for validateCard() → replace with verifyPaymentMethod()
5. Consolidate private helpers: remove validateAmount, validateCurrency, validateCard (helper) → create validateAll()
6. Compile check

**N07 — Regression Test Run:**
- Run PaymentServiceTests.cs (updated for new method names)
- Run OrderServiceTests.cs (caller)
- Run CheckoutTests.cs (caller)
- Run all other caller tests
- All must pass (rename is pure name change; behavior unchanged)

---

## B18-N9-008-005 — N08 and N09: PaymentServiceRefactoring

**N08 Gate B:**
- count(passing_tests_after) >= count(passing_tests_before) ✓
- No references to old method names in codebase ✓ (rename complete)
- validateAll() replaces three separate validators ✓
- PaymentService public interface updated (executeTransaction, verifyPaymentMethod) ✓

**N08 verdict: PASS**

**N09 output:**
```
{
  "connector_name": "PaymentServiceRefactoring",
  "flow": "BackendServiceFlow41",
  "sub_phase": "4.1",
  "transition_type": "refactoring",
  "gate_b_status": "PASS",
  "gate_b_condition": "all tests pass AND no old method name references remain",
  "files_generated": 4,
  "artifacts": [
    "PaymentService_refactored.cs",
    "PaymentServiceTests_updated.cs",
    "caller_updates.diff",
    "refactoring_summary.md"
  ],
  "n08_verdict": "PASS",
  "completion_message": "PaymentService refactored; 4 files updated"
}
```

---

## B18-N9-008-006 — rename + consolidate refactoring: combined operation

**rename and consolidate are both handled by BackendServiceFlow41 in one execution:**

- rename: N06 performs text replacement across all callers
- consolidate: N06 removes three methods and creates one consolidated method
- Both operations: checked for breakage at N07 (regression tests)
- Both verified at N08 (Gate B — test regression + no old names)

**No separate flow is needed for rename vs consolidate.** BackendServiceFlow41 handles both in the N06 execution step. The refactoring plan (N04) specifies all operations; N06 executes them all.
