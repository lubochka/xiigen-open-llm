# Cross-Layer Synthesis (002)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## CLS-B28-002 — Gap path: "Deploy to staging for testing"

### Request
"Deploy the build to staging for testing."

### Predicate Set
[] → GapHandler → SCHEDULE_CAPABILITY_LEARNING

### Cross-Layer Trace

**Compiler (0-A)**: "deploy" → operational gap (not IMPLEMENT). "staging" → operational context. "testing" → no test-writing verb. No abbreviation QA. Zero predicates compiled.
**Router (0-B)**: Zero predicates → GapHandler path. DomainId: "software-development".
**Knowledge Edge (0-C)**: Gap edges: deploy→gap (operational verb), staging→gap (environment noun without QA prefix), testing→gap (noun without test-writing verb).
**Flow Factory (0-D)**: No flow instantiated. GapHandler emits SCHEDULE_CAPABILITY_LEARNING event to subproject 16 (AutonomousLearningPipeline).
**Transition Orchestrator (Layer 1)**: No TransitionType. Gap verdict returned.
**Nine-Node Flow (Layer 2)**: No nine-node flow. GapHandler response only.
**Teaching Unit (Layer 3)**: Contrast — "deploy to staging" ≠ IMPLEMENT. "for testing" ≠ TEST. No QA abbreviation → gap.

### Key Teaching Points
1. "deploy" is operational gap. IMPLEMENT requires code-writing verbs.
2. "testing" as a purpose clause without test-writing verb → gap.
3. "staging" alone (without QA prefix) → gap.
4. Zero predicates → SCHEDULE_CAPABILITY_LEARNING emitted.

### Contrast: What would fire predicates
- "Implement the retry logic" → IMPLEMENT
- "Write smoke tests for the staging build" → TEST
- "QA the staging build" → QA
- "Deploy to staging" alone → gap
