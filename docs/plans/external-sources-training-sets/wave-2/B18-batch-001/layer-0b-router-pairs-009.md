# Layer 0-B — Router Pairs (009)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: router

## B18-0B-009-001 — [IMPLEMENT, ARCHITECTURE] refactoring → 4.1 + 3.1

**Scenario:** OrderServiceRefactoring with pattern redesign

**Router input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: refactoring
- Vocabulary: "refactor", "repository", "pattern", "architecture" → 4.1 + 3.1
- Request: "Refactor the OrderRepository to follow the repository pattern architecture."

**Routing logic:**
1. IMPLEMENT (refactoring, 4.1 vocabulary: "repository") → BackendServiceFlow41, sub-phase 4.1
2. ARCHITECTURE ("pattern", "architecture") → ArchitectureFlow31, sub-phase 3.1

**Router output:**
- Flows: [BackendServiceFlow41, ArchitectureFlow31]
- Sub-phases: [4.1, 3.1]
- TransitionType: refactoring

**Key distinction from 003:** This is refactoring (4.1) + architecture (3.1), NOT migration (4.4) + architecture (3.1).

---

## B18-0B-009-002 — refactoring + ARCHITECTURE: BackendServiceFlow41 not DataMigrationFlow44

**When predicates: ["IMPLEMENT", "ARCHITECTURE"] with TransitionType: refactoring:**
- IMPLEMENT routes to BackendServiceFlow41 (4.1) — NOT DataMigrationFlow44 (4.4)
- ARCHITECTURE routes to ArchitectureFlow31 (3.1)

**When predicates: ["IMPLEMENT", "ARCHITECTURE"] with TransitionType: feature_addition:**
- IMPLEMENT routes to DataMigrationFlow44 (4.4)
- ARCHITECTURE routes to ArchitectureFlow31 (3.1)

**TransitionType determines which IMPLEMENT flow is used.**

---

## B18-0B-009-003 — ARCHITECTURE execution order in refactoring context

**Request:** "Refactor the UserRepository to follow the unit of work pattern and write tests to verify."

**Router input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "TEST"]
- TransitionType: refactoring
- Vocabulary: "refactor", "repository" → 4.1; "pattern" → 3.1; "write tests" → 5.x

**Routing:**
- IMPLEMENT → BackendServiceFlow41, 4.1
- ARCHITECTURE → ArchitectureFlow31, 3.1
- TEST → UnitTestingFlow51, 5.x

**Execution order:**
1. ARCHITECTURE (define pattern to follow — before implementation)
2. IMPLEMENT (refactor following the pattern)
3. TEST (verify refactored interface)

**Router output:**
- Flows: [BackendServiceFlow41, ArchitectureFlow31, UnitTestingFlow51]
- Sub-phases: [4.1, 3.1, 5.x]

---

## B18-0B-009-004 — "clean architecture" → ARCHITECTURE (3.1), IMPLEMENT → BackendServiceFlow41

**Request:** "Refactor the service layer to comply with clean architecture principles."

**Router input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: refactoring
- Vocabulary: "refactor", "service layer" → 4.1; "architecture" → 3.1

**Router output:**
- IMPLEMENT → BackendServiceFlow41, 4.1
- ARCHITECTURE → ArchitectureFlow31, 3.1
- Flows: [BackendServiceFlow41, ArchitectureFlow31]

---

## B18-0B-009-005 — refactoring + pattern + document → three flows

**Request:** "Refactor the OrderService following the CQRS (Command Query Responsibility Segregation) pattern and document the new interface."

**Router input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "DOCUMENT"]
- TransitionType: refactoring
- Vocabulary: "refactor", "service" → 4.1; "pattern" → 3.1; "document" → 2.1

**Router output:**
- IMPLEMENT → BackendServiceFlow41, 4.1
- ARCHITECTURE → ArchitectureFlow31, 3.1
- DOCUMENT → DocumentationFlow21, 2.1
- Flows: [BackendServiceFlow41, ArchitectureFlow31, DocumentationFlow21]

---

## B18-0B-009-006 — B18 refactoring routing summary

**B18 refactoring flow combinations:**

| Predicates | Flows | Sub-phases | TransitionType |
|---|---|---|---|
| [IMPLEMENT] | BackendServiceFlow41 | 4.1 | refactoring |
| [IMPLEMENT, ARCHITECTURE] | BackendServiceFlow41 + ArchitectureFlow31 | 4.1 + 3.1 | refactoring |
| [IMPLEMENT, TEST] | BackendServiceFlow41 + UnitTestingFlow51 | 4.1 + 5.x | refactoring |
| [IMPLEMENT, DOCUMENT] | BackendServiceFlow41 + DocumentationFlow21 | 4.1 + 2.1 | refactoring |
| [IMPLEMENT, ARCHITECTURE, TEST] | BackendServiceFlow41 + ArchitectureFlow31 + UnitTestingFlow51 | 4.1 + 3.1 + 5.x | refactoring |
| [IMPLEMENT, ARCHITECTURE, DOCUMENT] | BackendServiceFlow41 + ArchitectureFlow31 + DocumentationFlow21 | 4.1 + 3.1 + 2.1 | refactoring |
