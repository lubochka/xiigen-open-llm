# Layer 0-C — Knowledge-Edge Pairs (001)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: knowledge-edge

## B18-0C-001-001 — B18 knowledge graph: core edges

**B18 knowledge graph has three categories of edges:**

1. **Predicate edges:** trigger word → predicate (e.g., "refactor" → IMPLEMENT)
2. **Routing edges:** predicate + TransitionType + vocabulary → sub-phase + flow
3. **TransitionType edges:** action verb → TransitionType classification

**Core edges for B18:**
- "refactor" → IMPLEMENT (predicate edge)
- IMPLEMENT + refactoring + 4.1 vocabulary → BackendServiceFlow41 (routing edge)
- "refactor" → refactoring (TransitionType edge)
- "structure" → ARCHITECTURE (predicate edge, absolute)
- "seed" → IMPLEMENT (predicate edge)
- IMPLEMENT + feature_addition + 4.4 vocabulary → DataMigrationFlow44 (routing edge)

---

## B18-0C-001-002 — Absolute edge vs contextual edge

**Absolute edges (cannot be suppressed):**
- "structure" → ARCHITECTURE (Rule 3, absolute)
- "refactor" → IMPLEMENT (Rule 1, absolute)
- "refactor" → refactoring TransitionType (Rule 9, absolute)
- DomainId → "software-development" (Rule 14, absolute)

**Contextual edges (require context to fire):**
- "schema" → ARCHITECTURE (only when paired with structural noun — Rule 16)
- "load" → 4.4 routing (only in data context)
- "move" → refactoring TransitionType (only in code context; data move = feature_addition)

---

## B18-0C-001-003 — Edge conflict resolution

**When two edges conflict, the hierarchy applies:**
- TransitionType: refactoring > feature_addition > documentation
- Sub-phase: explicit vocabulary wins over implicit context
- Predicate: additive (both fire; no suppression between predicates)

**Example conflict:** "Write migration to consolidate tables"
- "write migration" → feature_addition edge
- "consolidate" → refactoring edge
- Resolution: feature_addition wins (Rule 2 — "write migration" creates new artifact, overrides consolidate)

---

## B18-0C-001-004 — Knowledge edge example: OrderHistoryMigration

**Request tokens and their knowledge edges:**

| Token | Edge type | Target |
|---|---|---|
| "migrate" | predicate edge | IMPLEMENT |
| "order history records" | routing vocabulary edge | 4.4 sub-phase |
| "flat table" | routing vocabulary edge | 4.4 (database vocabulary) |
| "normalized orders/items/shipments" | routing vocabulary edge | 4.4 |
| No "structure" | (absent) | ARCHITECTURE does not fire |
| No "refactor" | (absent) | TransitionType: feature_addition |

**Result edges:**
- IMPLEMENT → DataMigrationFlow44 (4.4) via routing edge
- TransitionType: feature_addition (no refactoring verb)

---

## B18-0C-001-005 — Knowledge graph: B18 overview

**B18 knowledge graph nodes:**
- Predicate nodes: IMPLEMENT, ARCHITECTURE, TEST, DOCUMENT, SECURITY
- Flow nodes: DataMigrationFlow44, BackendServiceFlow41, ArchitectureFlow31, UnitTestingFlow51, DocumentationFlow21, SecurityReviewFlow
- Sub-phase nodes: 4.4, 4.1, 3.1, 5.x, 2.1, security
- TransitionType nodes: refactoring, feature_addition, documentation, no contribution, none

**B18 knowledge graph connections:**
- Token vocabulary nodes connect to predicate nodes (predicate edges)
- Predicate + TransitionType + vocabulary nodes connect to flow nodes (routing edges)
- Action verb nodes connect to TransitionType nodes (classification edges)

---

## B18-0C-001-006 — Knowledge edge: "migrate" disambiguation

**"migrate" has two distinct knowledge edges depending on context:**

**Edge 1: migrate (code context)**
- "migrate the codebase to .NET 9" → IMPLEMENT, may be refactoring or feature_addition
- Context: programming language, framework, code restructuring
- Sub-phase: 4.1 (code migration)

**Edge 2: migrate (data context)**
- "migrate customer records to new schema" → IMPLEMENT, feature_addition
- Context: data records, database tables, schema
- Sub-phase: 4.4 (data migration)

**Disambiguation rule:** "migrate" + data vocabulary (records, data, table, database) → 4.4. "migrate" + code vocabulary (codebase, framework, language) → 4.1.
