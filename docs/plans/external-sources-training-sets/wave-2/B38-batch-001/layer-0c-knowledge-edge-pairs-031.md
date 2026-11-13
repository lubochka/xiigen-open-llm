# Layer 0-C — Knowledge-Edge Pairs (031)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-031-001 — MigrationGuide context boundary

### MigrationGuide terms: context only (no predicate)
| Term | Category | Notes |
|---|---|---|
| migration | Process noun | No predicate |
| migration guide | Document noun | No predicate (document artifact only when "document" verb present) |
| upgrade path | Path noun | No predicate |
| migration step | Step noun | No predicate |
| rollback plan | Plan noun | No predicate |
| migration window | Window noun | No predicate |
| data migration | Migration type | No predicate |

### MigrationGuide inputs producing knowledge_edge
| Input | Analysis |
|---|---|
| "Prepare the migration guide for v2." | "Prepare" not in lexicon → [] |
| "Schedule the migration window." | "Schedule" not in lexicon → [] |
| "Plan the data migration rollback." | "Plan" not in lexicon; "rollback" context → [] |

---

## B38-0C-031-002 — UpgradeGuide context boundary

### UpgradeGuide terms: context only
| Term | Category | Notes |
|---|---|---|
| upgrade | Process noun | No predicate |
| upgrade guide | Document noun | No predicate alone |
| upgrade path | Path noun | No predicate |
| upgrade checklist | Checklist noun | No predicate (checklist is B38 gap) |
| upgrade window | Window noun | No predicate |
| version upgrade | Upgrade type | No predicate |

### UpgradeGuide inputs producing knowledge_edge
| Input | Analysis |
|---|---|
| "Schedule the v4 upgrade window." | "Schedule" not in lexicon → [] |
| "Distribute the upgrade guide to customers." | "Distribute" not in lexicon → [] + gap: distribute |
| "Plan the upgrade rollout." | "Plan" not in lexicon; "rollout" gap → [] + gap: rollout |

---

## B38-0C-031-003 — Migration vs. upgrade: "distribute" and "schedule" as gaps

"Distribute" and "schedule" are gap verbs in B38 — they never fire predicates.
"Rollout" remains gap in all upgrade/migration contexts.

| Sentence | PredicateSet |
|---|---|
| "Distribute the migration guide." | [] + gap: distribute |
| "Distribute the upgrade notes." | [] + gap: distribute |
| "Schedule the migration." | [] (schedule not in lexicon) |
| "Document the migration guide." | ["DOCUMENT"] (document verb fires) |
| "Review the upgrade guide." | ["QA"] (review fires) |
