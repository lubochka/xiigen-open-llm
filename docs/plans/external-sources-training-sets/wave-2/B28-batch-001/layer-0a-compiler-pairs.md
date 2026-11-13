# Layer 0-A — Compiler Pairs
# Batch: B28-batch-001
# Scenario: B28 — QA automation and DevOps — release pipeline QA, sub-phase 7.1
# Contract: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: compiler

---

## Pair B28B1-WDA-compiler-001 — QA + IMPLEMENT: QA pipeline automation

```
source_id: xiigen-b28-compiler-001
transition_type: feature_addition
phase: 7
sub_phase: 7.1
coverage_status: ready
pair_kind: compiler
```

A:
  user_request: "Implement the automated QA pipeline for the release process and build the smoke test harness."

B:
  DocumentId: "B28-compiler-001-prompt"
  DocumentTitle: "Automated QA pipeline and smoke test harness"
  CompiledText: "Implement the automated QA pipeline for the release process and build the smoke test harness."
  CompiledPredicateIds: ["QA", "IMPLEMENT", "TEST"]
  SourceStatus: "ready"

  Predicate evidence:
    "QA" — QA predicate fires on "QA"
    "implement" → IMPLEMENT
    "pipeline" — ARCHITECTURE? "pipeline" alone insufficient; no design/structure action verb
    "build" → IMPLEMENT
    "smoke test harness" — TEST fires on "test" (smoke test)
    "harness" — IMPLEMENT? "harness" is a code artifact term → IMPLEMENT via "build the harness"
    "automated" — gap
    "release process" — gap (from B38 lessons: release vocabulary is gap)
    Result: ["QA", "IMPLEMENT", "TEST"]

---

## Pair B28B1-WDA-compiler-002 — QA only: QA the deployment

```
source_id: xiigen-b28-compiler-002
transition_type: feature_addition
phase: 7
sub_phase: 7.1
coverage_status: ready
pair_kind: compiler
```

A:
  user_request: "QA the deployment artifacts and verify the staging environment health checks."

B:
  DocumentId: "B28-compiler-002-prompt"
  DocumentTitle: "QA of deployment artifacts and staging health checks"
  CompiledText: "QA the deployment artifacts and verify the staging environment health checks."
  CompiledPredicateIds: ["QA", "TEST"]
  SourceStatus: "ready"

  Predicate evidence:
    "QA" — QA predicate fires
    "verify" — TEST fires on "verify"
    "deployment artifacts" — gap (deployment vocabulary from B38)
    "staging environment" — gap
    "health checks" — gap (DevOps/infrastructure term)
    Result: ["QA", "TEST"]

  Boundary: "verify" fires TEST independent of the object. "Verify the health checks"
  → TEST fires because "verify" is a TEST predicate term regardless of what is being verified.

---

## Pair B28B1-WDA-compiler-003 — QA + ARCHITECTURE: QA infrastructure design

```
source_id: xiigen-b28-compiler-003
transition_type: feature_addition
phase: 7
sub_phase: 7.1
coverage_status: ready
pair_kind: compiler
```

A:
  user_request: "Design the QA automation architecture and structure the test infrastructure layer."

B:
  DocumentId: "B28-compiler-003-prompt"
  DocumentTitle: "QA automation architecture and test infrastructure design"
  CompiledText: "Design the QA automation architecture and structure the test infrastructure layer."
  CompiledPredicateIds: ["QA", "ARCHITECTURE", "TEST"]
  SourceStatus: "ready"

  Predicate evidence:
    "QA" — QA fires
    "design" → ARCHITECTURE
    "architecture" → ARCHITECTURE
    "structure" → ARCHITECTURE
    "infrastructure layer" → ARCHITECTURE fires on "layer" and "infrastructure" in architectural context
    "test infrastructure" — TEST fires on "test"
    "automation" — gap
    Result: ["QA", "ARCHITECTURE", "TEST"]
