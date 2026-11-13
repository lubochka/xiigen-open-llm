# Layer 0-C — Knowledge-Edge Pairs (010)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: knowledge-edge

## B18-0C-010-001 — ARCHITECTURE fires: complete trigger list

**All words and phrases that fire ARCHITECTURE predicate:**

**Absolute triggers (always fire, Rule 3/7):**
| Trigger | Rule | Notes |
|---|---|---|
| structure | Rule 3 | Absolute — cannot be suppressed |
| architecture | Rule 7 | Absolute |
| pattern | Rule 7 | Absolute — "repository pattern", "architecture pattern" |
| design (structural noun) | Rule 7 | "design the schema", "design the architecture" |
| redesign | Rule 7 | Contains "design" structural use |

**Contextual triggers (require pairing):**
| Trigger | Required pairing | Notes |
|---|---|---|
| schema | Paired with design/architecture/structure noun | Rule 16 — borderline |
| blueprint | Structural design context | Rare in B18 |
| model (architectural) | Paired with design noun | "design the data model" |

---

## B18-0C-010-002 — ARCHITECTURE sub-phase: 3.1 vs 3.5

**Sub-phase 3.1 (Application/Domain Architecture):**
- Application structure → 3.1
- Domain architecture → 3.1
- Database schema architecture → 3.1 (most common in B18)
- Service architecture → 3.1
- Data model design → 3.1
- Repository pattern → 3.1

**Sub-phase 3.5 (Infrastructure Architecture):**
- Infrastructure pattern → 3.5
- Cloud architecture → 3.5
- Deployment architecture → 3.5
- Container orchestration → 3.5
- Network architecture → 3.5

**B18 default:** ALL ARCHITECTURE in B18 routes to 3.1 unless explicit infrastructure vocabulary present. B18 is a development scenario (not infrastructure).

---

## B18-0C-010-003 — "pattern" fires ARCHITECTURE in all contexts

**"pattern" fires ARCHITECTURE regardless of which pattern:**

| Pattern phrase | ARCHITECTURE fires? | Sub-phase |
|---|---|---|
| "repository pattern" | YES | 3.1 |
| "unit of work pattern" | YES | 3.1 |
| "CQRS pattern" (Command Query Responsibility Segregation) | YES | 3.1 |
| "factory pattern" | YES | 3.1 |
| "singleton pattern" | YES | 3.1 |
| "adapter pattern" | YES | 3.1 |
| "migration pattern" | YES | 3.1 |
| "event sourcing pattern" | YES | 3.1 |
| "clean architecture pattern" | YES | 3.1 |

**All design patterns fire ARCHITECTURE.** The compiler is literal on "pattern" — no contextual suppression.

---

## B18-0C-010-004 — "design" as structural noun vs "design" as activity verb

**"design" (structural noun — fires ARCHITECTURE):**
- "Design the target schema" → ARCHITECTURE
- "Design the migration architecture" → ARCHITECTURE
- "Design the data model" → ARCHITECTURE
- "Design the database structure" → ARCHITECTURE
- Pattern: "design [the] [noun]" where noun is architectural artifact

**"design" (not structural noun — does NOT fire ARCHITECTURE):**
- "The design was good" (adjective/general usage — borderline; compiler may still fire)
- "By design, the migration..." (prepositional phrase — may not fire)
- **B18 conservative rule:** If "design" appears, ARCHITECTURE fires unless clearly non-structural (adverbial usage).

---

## B18-0C-010-005 — ARCHITECTURE + IMPLEMENT combined: routing for 3.1

**When ARCHITECTURE fires alongside IMPLEMENT:**

**ARCHITECTURE fires ArchitectureFlow31 (3.1):**
- Always sub-phase 3.1 in B18 (default)
- ArchitectureFlow31 runs concurrently with or before DataMigrationFlow44/BackendServiceFlow41
- ArchitectureFlow31 outputs: schema design, architecture diagrams, structural specifications

**IMPLEMENT receives ArchitectureFlow31 output:**
- DataMigrationFlow44 uses the schema design to write the migration
- BackendServiceFlow41 uses the architecture to guide refactoring

**Ordering:**
- ArchitectureFlow31 (3.1) → typically before or concurrent with IMPLEMENT flow
- IMPLEMENT flow uses architecture output as input specification

---

## B18-0C-010-006 — ARCHITECTURE trigger summary: complete B18 trigger map

**B18 ARCHITECTURE trigger map:**

| Trigger category | Words | Always fires? | Default sub-phase |
|---|---|---|---|
| Structure token | structure | YES (Rule 3) | 3.1 |
| Architecture noun | architecture | YES (Rule 7) | 3.1 |
| Pattern noun | pattern | YES (Rule 7) | 3.1 |
| Design structural | design, redesign | YES (Rule 7) | 3.1 |
| Schema paired | schema + design/architecture/structure | YES when paired | 3.1 |
| Infrastructure | infrastructure + architecture | YES | 3.5 |
| Deployment | deployment + architecture/pattern | YES | 3.5 |

**B18 has no 3.5 scenarios in standard cases.** All ARCHITECTURE in B18 = 3.1.
