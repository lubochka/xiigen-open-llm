# Layer 0-A — Compiler Pairs
# Batch: B20-batch-001
# Scenario: B20 — Unit testing — writing unit and integration tests, sub-phase 5.1
# Contract: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: compiler

---

## Pair B20B1-WDA-compiler-001 — pure TEST: unit tests for service logic

```
source_id: xiigen-b20-compiler-001
transition_type: feature_addition
phase: 5
sub_phase: 5.1
coverage_status: ready
pair_kind: compiler
```

A:
  user_request: "Write unit tests for the PaymentCalculationService covering all edge cases."

B:
  DocumentId: "B20-compiler-001-prompt"
  DocumentTitle: "Unit tests for PaymentCalculationService"
  CompiledText: "Write unit tests for the PaymentCalculationService covering all edge cases."
  CompiledPredicateIds: ["TEST"]
  SourceStatus: "ready"

  Predicate evidence:
    "write" — IMPLEMENT-adjacent but "write tests" compound → TEST via test co-occurrence
    "unit tests" — TEST fires on "tests" and "unit tests" compound
    "PaymentCalculationService" — not in any predicate (service name, not vocabulary)
    "edge cases" — not in any predicate (testing methodology term; gap)
    "covering" — not in any predicate
    Result: ["TEST"] via "unit tests"

  Note: "write" alone fires IMPLEMENT but "write unit tests" as a compound is parsed
  as a TEST action. The noun "unit tests" is the object — test vocabulary dominates.

---

## Pair B20B1-WDA-compiler-002 — TEST + API_CONTRACT: integration tests with mock

```
source_id: xiigen-b20-compiler-002
transition_type: feature_addition
phase: 5
sub_phase: 5.1
coverage_status: ready
pair_kind: compiler
```

A:
  user_request: "Add integration tests for the OrderService API contract and mock the external payment interface."

B:
  DocumentId: "B20-compiler-002-prompt"
  DocumentTitle: "Integration tests for OrderService API contract"
  CompiledText: "Add integration tests for the OrderService API contract and mock the external payment interface."
  CompiledPredicateIds: ["TEST", "API_CONTRACT"]
  SourceStatus: "ready"

  Predicate evidence:
    "integration tests" — TEST fires on "tests" (integration tests = tests)
    "API contract" — API_CONTRACT fires on "contract" in API context
    "interface" — API_CONTRACT fires on "interface"
    "add" — IMPLEMENT? "add" fires IMPLEMENT in some contexts. Here "add tests" → TEST dominates.
      Correction: "add" does fire IMPLEMENT in general, but "add integration tests" → TEST.
      Does IMPLEMENT also fire? "add" → IMPLEMENT; "tests" → TEST. Both fire.
    Result: ["TEST", "API_CONTRACT", "IMPLEMENT"] — three predicates.

  Correction: "add" fires IMPLEMENT independently of the "tests" object.
  CompiledPredicateIds: ["TEST", "API_CONTRACT", "IMPLEMENT"]

  Final:
    CompiledPredicateIds: ["TEST", "API_CONTRACT", "IMPLEMENT"]
    SourceStatus: "ready"

  Teaching: "add" fires IMPLEMENT even when the object is "tests". Predicate compilation
  is not suppressed by the object noun. Both TEST (from "tests") and IMPLEMENT (from "add")
  fire independently. "contract" + "interface" fire API_CONTRACT.

---

## Pair B20B1-WDA-compiler-003 — TEST + SECURITY: auth service test with token validation

```
source_id: xiigen-b20-compiler-003
transition_type: feature_addition
phase: 5
sub_phase: 5.1
coverage_status: ready
pair_kind: compiler
```

A:
  user_request: "Write the test suite for the authentication service and verify the JWT token validation logic."

B:
  DocumentId: "B20-compiler-003-prompt"
  DocumentTitle: "Test suite for authentication service JWT validation"
  CompiledText: "Write the test suite for the authentication service and verify the JWT token validation logic."
  CompiledPredicateIds: ["TEST", "SECURITY"]
  SourceStatus: "ready"

  Predicate evidence:
    "test suite" — TEST fires on "test" and "suite" (suite ≈ test collection)
    "verify" — TEST fires on "verify" (verify/assert/validate are TEST-adjacent)
    "authentication service" — SECURITY fires on "authentication"
    "JWT" (JSON Web Token) — SECURITY fires on JWT/token/auth security terms
    "token validation" — SECURITY fires on "validation" in security context
    "write" → IMPLEMENT? "write the test suite" → TEST dominates, but "write" alone fires IMPLEMENT.
      Three predicates: TEST + SECURITY + IMPLEMENT.

  Correction: "write" fires IMPLEMENT independently.
  CompiledPredicateIds: ["TEST", "SECURITY", "IMPLEMENT"]

  Final:
    CompiledPredicateIds: ["TEST", "SECURITY", "IMPLEMENT"]
    SourceStatus: "ready"

  Teaching: Multi-predicate accumulation — "write" (IMPLEMENT), "test suite"/"verify" (TEST),
  "authentication"/"JWT"/"token" (SECURITY) — three independent predicates fire. All three
  produce independent routing decisions.
