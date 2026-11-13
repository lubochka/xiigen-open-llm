# Layer 1 — Transition Orchestrator Pairs (001)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: transition-orchestrator

## B18-L1-001-001 — B18 orchestrator overview

**The transition orchestrator sequences flow execution for B18 scenarios.**

**Orchestrator inputs:**
- Compiled predicates + TransitionType
- Router output (flows + sub-phases)
- Factory output (flow instances + dependencies)

**Orchestrator responsibilities:**
1. Order flow execution (which flow runs first, second, etc.)
2. Manage parallel execution (which flows can run concurrently)
3. Handle dependency constraints (TEST after IMPLEMENT; ARCHITECTURE before IMPLEMENT)
4. Manage rollback on flow failure
5. Collect flow outputs and pass to N08 for Gate B evaluation

---

## B18-L1-001-002 — B18 orchestration ordering rules

**Standard orchestration ordering for B18:**

1. **ARCHITECTURE (ArchitectureFlow31) first** — schema/pattern design before implementation
2. **IMPLEMENT (DataMigrationFlow44 or BackendServiceFlow41) second** — primary artifact creation
3. **SECURITY (SecurityReviewFlow) third** — security review after implementation
4. **TEST (UnitTestingFlow51) fourth** — tests after implementation exists
5. **DOCUMENT (DocumentationFlow21) variable** — can be parallel with IMPLEMENT or sequential

---

## B18-L1-001-003 — Single flow orchestration

**Single flow (IMPLEMENT only):**
```
Orchestrator sequence:
1. [DataMigrationFlow44 OR BackendServiceFlow41]
   → Execute
   → N08 Gate B evaluation
   → N09 output
```

No parallelism needed. No dependency management. Simple sequential execution.

---

## B18-L1-001-004 — Two-flow orchestration: IMPLEMENT+ARCHITECTURE

**Two flows (IMPLEMENT + ARCHITECTURE):**
```
Orchestrator sequence:
1. ArchitectureFlow31 → produces schema specification
2. DataMigrationFlow44 (receives schema spec) → produces migration
3. N08 evaluates both: schema complete? migration complete?
4. N09 output
```

**Dependency:** DataMigrationFlow44 depends_on ArchitectureFlow31.
**Parallel potential:** None for these two (sequential dependency).

---

## B18-L1-001-005 — Two-flow orchestration: IMPLEMENT+DOCUMENT

**Two flows (IMPLEMENT + DOCUMENT):**
```
Orchestrator sequence (parallel eligible):
Parallel track A: DataMigrationFlow44 → migration script
Parallel track B: DocumentationFlow21 → schema change documentation
Both complete → N08 evaluation
```

**Dependency:** DocumentationFlow21 does NOT depend on DataMigrationFlow44 for schema documentation. They can run in parallel.
**Exception:** If documentation is "as-built" (documents the final implementation), DocumentationFlow21 must wait.

---

## B18-L1-001-006 — Orchestrator metadata output

**Orchestrator output to nine-node flow:**
```
OrchestratorOutput {
  execution_plan: [
    { step: 1, flow: "ArchitectureFlow31", parallel_with: null },
    { step: 2, flow: "DataMigrationFlow44", parallel_with: ["DocumentationFlow21"] },
    { step: 3, flow: "UnitTestingFlow51", parallel_with: null }
  ],
  connector_name: string,
  transition_type: string,
  domain_id: "software-development",
  gate_b_conditions: [...],
  rollback_plan: [...]
}
```
