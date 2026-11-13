# Cross-Layer Synthesis (001)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## CLS-B28-001 — "Implement the payment retry logic and QA sign-off"

### Request
"Implement the payment retry logic and get QA sign-off before merging."

### Predicate Set
["IMPLEMENT", "QA"]

### Cross-Layer Trace

**Compiler (0-A)**: Scans "implement" → IMPLEMENT; "QA sign-off" → QA. Two predicates compiled.
**Router (0-B)**: IMPLEMENT → sub-phase 4.1 (ImplementationFlow41); QA → sub-phase 7.3 (QAGatingFlow73). No release vocabulary → 7.3.
**Knowledge Edge (0-C)**: "implement" fires IMPLEMENT (code-writing verb). "QA sign-off" fires QA (compound noun with abbreviation). Edges: implement→IMPLEMENT, QA-compound-noun→QA.
**Flow Factory (0-D)**: [ImplementationFlow41, QAGatingFlow73]. TransitionType: feature_addition (IMPLEMENT present). QA does not drive TransitionType.
**Transition Orchestrator (Layer 1)**: Sequential. IMPLEMENT completes first (code written, reviewed). QA executes last (validates complete output).
**Nine-Node Flow (Layer 2)**: ImplementationFlow41 executes (payment retry logic built, code reviewed). QAGatingFlow73 executes last (gate passes, PASS verdict).
**Teaching Unit (Layer 3)**: IMPLEMENT+QA — two-predicate. QA does not override IMPLEMENT's TransitionType.

### Key Teaching Points
1. "implement" → IMPLEMENT; "QA sign-off" → QA. Both fire independently.
2. QA always last in sequential execution.
3. TransitionType: feature_addition (from IMPLEMENT). QA never drives TransitionType.
4. Sub-phase 7.3 (no release vocabulary) not 7.1.
