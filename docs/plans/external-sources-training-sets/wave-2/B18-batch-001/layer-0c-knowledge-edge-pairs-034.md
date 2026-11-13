# Layer 0-C — Knowledge-Edge Pairs (034)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-034-001 — Edge map: "backfill" token in DDL context

**"backfill" token behavior:**

| Phrase | Edge | TransitionType |
|---|---|---|
| "backfill nulls" | IMPLEMENT + 4.4 | feature_addition |
| "backfill default values" | IMPLEMENT + 4.4 | feature_addition |
| "backfill existing rows" | IMPLEMENT + 4.4 | feature_addition |
| "backfill + rename column" | IMPLEMENT + 4.4 | refactoring (rename wins) |

**Rule:** "backfill" is a data operation that maps to feature_addition + 4.4. It does NOT fire refactoring.

---

## B18-0C-034-002 — Edge map: "populate" token in DDL context

**"populate" token behavior:**

| Phrase | Edge | TransitionType |
|---|---|---|
| "populate sequence cache" | IMPLEMENT + 4.4 | feature_addition |
| "populate the employee ID sequence" | IMPLEMENT + 4.4 | feature_addition |
| "populate default values" | IMPLEMENT + 4.4 | feature_addition |

**Rule:** "populate" = seeding-equivalent vocabulary → always feature_addition + 4.4. Never 4.1 (service code) or 5.x (tests).

---

## B18-0C-034-003 — Edge map: "add" vs "create" vs "write" in DDL context

**Action verb edge comparison:**

| Verb | Edge | TransitionType |
|---|---|---|
| "write a migration to add X" | IMPLEMENT + feature_addition + 4.4 | feature_addition (Rule 2) |
| "create X" (DDL artifact) | IMPLEMENT + feature_addition + 4.4 | feature_addition (Rule 2) |
| "add X constraint" | IMPLEMENT + feature_addition + 4.4 | feature_addition |
| "build X" (database artifact) | IMPLEMENT + feature_addition + 4.4 | feature_addition |
| "implement X migration" | IMPLEMENT + feature_addition + 4.4 | feature_addition (Rule 2) |

---

## B18-0C-034-004 — Edge map: "reset" token in DDL context

**"reset" token behavior:**

| Phrase | Edge | Notes |
|---|---|---|
| "reset sequence" | IMPLEMENT + 4.4 | database DDL operation |
| "reset the index" | IMPLEMENT + 4.4 | data rebuild vocabulary |
| "reset database" | IMPLEMENT + 4.4 | data migration vocabulary |
| "refactor + reset" | IMPLEMENT + refactoring | "refactor" wins |

**Rule:** "reset" in database context is a feature_addition + 4.4 operation. It does not fire refactoring independently.

---

## B18-0C-034-005 — Edge summary: complete 025-036 disambiguation rules

**The 8 critical disambiguation rules for DDL migration edge firing:**

1. "trigger" (database object) ≠ SECURITY trigger. Only PII/GDPR/encrypted etc. fire SECURITY.
2. "index/sequence/enum/procedure/view" alone ≠ ARCHITECTURE. Only structure/architecture/pattern fire ARCHITECTURE.
3. "partitioning" alone ≠ ARCHITECTURE. Only "partition structure/pattern/architecture" fires.
4. "rebuild" on data/index → feature_addition + 4.4 (not refactoring like code rebuild).
5. "convert" in DDL → feature_addition (not refactoring).
6. "backfill" → feature_addition + 4.4 (not refactoring).
7. "populate" → feature_addition + 4.4 (never 4.1 or 5.x).
8. "structured" (adjective) ≠ "structure" (noun). Only "structure" noun fires ARCHITECTURE.
