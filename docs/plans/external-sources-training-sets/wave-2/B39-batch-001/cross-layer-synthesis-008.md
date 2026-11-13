# Cross-Layer Synthesis (008)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## CLS-B39-008 — Six-predicate: IMPLEMENT+UI+TEST+QA+DOCUMENT+ARCHITECTURE

### Request
"Design the accessibility testing architecture for the component library, implement the test runner harness, write E2E and visual regression tests for every modal and form component, document the test coverage plan and ARIA contract, and QA sign-off on the complete accessibility suite before shipping."

### Predicate Set
["IMPLEMENT", "UI", "TEST", "QA", "DOCUMENT", "ARCHITECTURE"]

### Cross-Layer Trace

**Compiler (0-A)**:
  "design the accessibility testing architecture" → ARCHITECTURE (architecture/design verb + architecture noun).
  "implement the test runner harness" → IMPLEMENT.
  "write E2E and visual regression tests" → TEST.
  "modal", "form component" → UI (two triggers → one UI predicate).
  "document the test coverage plan and ARIA contract" → DOCUMENT.
  "QA sign-off" → QA.
  Six predicates compiled.

**Router (0-B)**:
  ARCHITECTURE → 2.1 (ArchitectureDesignFlow21).
  IMPLEMENT → 4.1 (BackendServiceFlow41).
  TEST → 5.3 (E2ETestingFlow53).
  UI → 1.5 (UIInteractionFlow15).
  DOCUMENT → 6.2 (DocumentationFlow62).
  QA → 7.3 (QAGatingFlow73).
  Six sub-phases.

**Knowledge Edge (0-C)**:
  "design ... architecture" → ARCHITECTURE; "implement ... harness" → IMPLEMENT; "write E2E + visual regression tests" → TEST (compound); "modal + form component" → UI (deduplicated); "document ... plan and ARIA contract" → DOCUMENT; "QA sign-off" → QA. Six independent predicate edges. No gap warnings for this request.

**Flow Factory (0-D)**:
  [ArchitectureDesignFlow21, BackendServiceFlow41, E2ETestingFlow53, UIInteractionFlow15, DocumentationFlow62, QAGatingFlow73].
  TransitionType: feature_addition (IMPLEMENT present).
  QA flow last.

**Transition Orchestrator (Layer 1)**:
  Phase 1: ArchitectureDesignFlow21 — design architecture (parallel with nothing; must complete first).
  Phase 2: BackendServiceFlow41 — implement harness (depends on architecture).
  Phase 3: E2ETestingFlow53 + UIInteractionFlow15 (parallel) — write tests; UI coverage.
  Phase 4: DocumentationFlow62 — document coverage plan and ARIA contract.
  Phase 5: QAGatingFlow73 — QA validates: architecture documented, harness built, tests pass, UI coverage confirmed, documentation complete.
  TransitionType: feature_addition.

**Nine-Node Flow (Layer 2)**: Architecture designed (2.1). Harness built (4.1). 12 E2E tests + visual regression tests written (5.3). Modal + form UI coverage confirmed (1.5). ARIA contract documented (6.2). QAGatingFlow73: PASS.

**Teaching Unit (Layer 3)**: Six predicates from six clauses. ARCHITECTURE precedes IMPLEMENT. QA is always the final arbiter. DOCUMENT fires on explicit documentation verbs. IMPLEMENT drives feature_addition regardless of how many other predicates are present.

### Key Teaching Points
1. Six predicates → six independent flows → six sub-phases. No collapse.
2. ARCHITECTURE fires on "design the ... architecture" → routes to 2.1 (ArchitectureDesignFlow21).
3. DOCUMENT fires on "document the test coverage plan" → routes to 6.2 (DocumentationFlow62).
4. IMPLEMENT present → feature_addition overrides all other TransitionType defaults.
5. QA gate validates the complete six-flow work package. It is always the final flow.
6. UI deduplication: "modal + form component" → one UI predicate, one routing decision to 1.5.
7. Execution order: ARCHITECTURE → IMPLEMENT → TEST+UI (parallel) → DOCUMENT → QA.
