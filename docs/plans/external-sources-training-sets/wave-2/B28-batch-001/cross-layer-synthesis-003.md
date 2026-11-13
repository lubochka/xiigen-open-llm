# Cross-Layer Synthesis (003)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## CLS-B28-003 — "Write tests, document, and QA sign-off" (TEST+DOCUMENT+QA)

### Request
"Write integration tests for the new API, document the endpoint contracts, and get QA sign-off."

### Predicate Set
["TEST", "DOCUMENT", "QA"]

### Cross-Layer Trace

**Compiler (0-A)**: "write integration tests" → TEST; "document the endpoint contracts" → DOCUMENT; "QA sign-off" → QA. Three predicates compiled.
**Router (0-B)**: TEST → sub-phase 5.1 (integration tests are regression-adjacent) or 5.3 (integration/E2E). DOCUMENT → DocumentationFlow62. QA → sub-phase 7.3 (QAGatingFlow73, no release vocabulary).
**Knowledge Edge (0-C)**: write-tests→TEST, document→DOCUMENT, QA-sign-off→QA. Three independent trigger edges.
**Flow Factory (0-D)**: [UnitTestingFlow51 or E2EFlow53, DocumentationFlow62, QAGatingFlow73]. TransitionType: documentation (IMPLEMENT absent; TEST+DOCUMENT → documentation TransitionType).
**Transition Orchestrator (Layer 1)**: Sequential: TEST completes first (tests written, passing), DOCUMENT completes second (API contracts documented), QA executes last (validates tests + docs).
**Nine-Node Flow (Layer 2)**: UnitTestingFlow51 executes (integration tests pass). DocumentationFlow62 executes (endpoint contracts documented). QAGatingFlow73 executes last (validates complete package — PASS).

### Key Teaching Points
1. Three distinct trigger clauses → three predicates. No collapse.
2. TransitionType: documentation (TEST+DOCUMENT without IMPLEMENT → documentation).
3. QA gate validates both the test suite AND the documentation.
4. QA always last regardless of request order.
