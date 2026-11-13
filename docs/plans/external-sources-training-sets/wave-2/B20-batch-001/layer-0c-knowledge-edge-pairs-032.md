# Layer 0-C — Knowledge Edge Pairs (032) — MarginCalculator: "trigger" gap and TransitionType summary
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E032-001 — Gap: "trigger" with reporting context → knowledge_edge

A: "Trigger the MarginCalculator for all products in the quarterly report."
B:
  CompiledPredicateIds: []
  GapVocabulary: ["trigger", "quarterly report", "all products"]
  Note: "trigger" is an execution verb. "quarterly report" is business context; no predicate.
  Execution verb gap family: run, execute, trigger, launch, invoke, start — all produce zero predicates.

---

## E032-002 — Gap: gross/net margin domain vocabulary — all zero-predicate

  "gross margin", "net margin", "operating margin", "profit margin", "contribution margin",
  "COGS (Cost of Goods Sold)", "revenue", "selling price", "cost price",
  "margin percentage", "margin calculation"
  None fire any predicate. Predicates come from action verbs only.

---

## E032-003 — TransitionType summary: TEST alone → documentation (not feature_addition)

When only TEST fires (no IMPLEMENT verb present):
  Request: "Validate, assert, and verify MarginCalculator handles 0% margin."
  CompiledPredicateIds: ["TEST"] — three verbs deduplicate to one TEST
  TransitionType: documentation
  Rationale: No construction verb is present. TEST alone specifies expected behaviour, which is documentation.
  Critical: Three TEST-firing verbs do NOT change TransitionType from documentation to feature_addition.
  Only the presence of an IMPLEMENT construction verb (write, create, build, develop, implement, add, extend, generate) changes TransitionType to feature_addition.
