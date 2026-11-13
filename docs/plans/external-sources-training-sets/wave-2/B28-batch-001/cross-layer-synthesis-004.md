# Cross-Layer Synthesis (004)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## CLS-B28-004 — Six-predicate: design + implement + secure + test + document + QA

### Request
"Design the new authorization layer, implement the role-based access control, secure the endpoints, write tests, document the access model, and QA sign-off before shipping."

### Predicate Set
["ARCHITECTURE", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"]

### Cross-Layer Trace

**Compiler (0-A)**: "design the new authorization layer" → ARCHITECTURE; "implement the role-based access control" → IMPLEMENT; "secure the endpoints" → SECURITY; "write tests" → TEST; "document the access model" → DOCUMENT; "QA sign-off" → QA. Six predicates compiled.
**Router (0-B)**: ARCHITECTURE → sub-phase 3.1 (app/domain vocabulary — authorization layer, RBAC); IMPLEMENT → 4.1; SECURITY → 6.x; TEST → 5.1; DOCUMENT → DocumentationFlow62; QA → 7.3.
**Knowledge Edge (0-C)**: Six independent trigger edges. RBAC fires ARCHITECTURE (named pattern). "secure endpoints" fires SECURITY. "QA sign-off" fires QA via abbreviation.
**Flow Factory (0-D)**: Six flows instantiated. TransitionType: feature_addition (IMPLEMENT present).
**Transition Orchestrator (Layer 1)**: Sequential with parallelism. ARCHITECTURE → IMPLEMENT (parallel with SECURITY) → TEST (parallel with DOCUMENT) → QA (always last).
**Nine-Node Flow (Layer 2)**: All six flows execute. QAGatingFlow73 executes last, validating: architecture ADRs, implementation, security controls, test suite, documentation. Verdict: PASS.

### Key Teaching Points
1. Six-predicate request → six flows. No cap.
2. RBAC fires ARCHITECTURE (named architectural pattern, sub-phase 3.1 for app-domain vocabulary).
3. TransitionType: feature_addition (IMPLEMENT drives it, not QA or DOCUMENT).
4. QA is the final arbiter — validates all five preceding work streams.
5. "quality assurance" spelled out would be gap; "QA sign-off" (abbreviation) fires QA.
