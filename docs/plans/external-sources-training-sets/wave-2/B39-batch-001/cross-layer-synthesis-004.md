# Cross-Layer Synthesis (004)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## CLS-B39-004 — Five-predicate: IMPLEMENT+TEST+UI+DOCUMENT+QA

### Request
"Build the design system button component with all variants, write interaction tests, fix button focus styling, document the component API, and QA sign-off."

### Predicate Set
["IMPLEMENT", "TEST", "UI", "DOCUMENT", "QA"]

### Cross-Layer Trace

**Compiler (0-A)**: "build" → IMPLEMENT; "write interaction tests" → TEST; "button component", "button focus styling" → UI; "document the component API" → DOCUMENT; "QA sign-off" → QA. Five predicates compiled.
**Router (0-B)**: IMPLEMENT → 4.1; TEST → 5.3 (interaction/E2E); UI → 1.5; DOCUMENT → DocumentationFlow62; QA → 7.3. Five sub-phases.
**Knowledge Edge (0-C)**: Five independent trigger edges. "button component" + "button focus styling" both fire UI (deduplicated to one UI predicate).
**Flow Factory (0-D)**: [ImplementationFlow41, E2ETestingFlow53, UIInteractionFlow15, DocumentationFlow62, QAGatingFlow73]. TransitionType: feature_addition. QA last.
**Transition Orchestrator (Layer 1)**:
  Phase 1: ImplementationFlow41 (build button component)
  Phase 2: E2ETestingFlow53 + UIInteractionFlow15 (parallel: tests + UI fix)
  Phase 3: DocumentationFlow62 (document API)
  Phase 4: QAGatingFlow73 (validates: implementation + tests + UI + documentation)
**Nine-Node Flow (Layer 2)**: All five flows complete. QAGatingFlow73 validates complete design system component. Verdict: PASS.

### Key Teaching Points
1. Five predicates from five distinct clauses. No collapse. All five flows instantiated.
2. UI deduplication: "button component" + "button focus styling" → one UI predicate.
3. TransitionType: feature_addition (IMPLEMENT present, overrides documentation default).
4. QA is final arbiter — validates all four preceding work streams. QA does not drive TransitionType.
5. DOCUMENT fires on "document the component API" (documentation verb + target).
