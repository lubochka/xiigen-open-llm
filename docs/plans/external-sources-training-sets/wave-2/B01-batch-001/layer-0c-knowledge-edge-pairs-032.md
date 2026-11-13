# Layer 0-C — Knowledge-Edge Pairs (032)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-032-001 — "integrate" is a gap term even with UI context

```
source_id: xiigen-b01-0c-032-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A-gap: "Integrate the product card with the inventory service and update the thumbnail."
  → "integrate" NOT in IMPLEMENT lexicon (confirmed gap from file 024 reference).
  → "thumbnail" NOT in any predicate lexicon.
  → "update" (non-code context) NOT in IMPLEMENT lexicon.
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE

A-fires: "Build a product card component that displays inventory status as a color badge."
  → "build" fires IMPLEMENT. "component" fires UI. "color" fires UI.
  → CompiledPredicateIds: ["UI", "IMPLEMENT"] → FLOW_EXECUTING

Gap term list reinforced: "integrate", "update" (non-code), "support", "allow", "run", "automate" — none in IMPLEMENT lexicon.

---

## B01-0C-032-002 — "automate" is gap; "write" fires IMPLEMENT

```
source_id: xiigen-b01-0c-032-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A-gap: "Automate the image gallery lazy loading when a user scrolls down."
  → "automate" NOT in any predicate lexicon.
  → "lazy loading" NOT in any predicate lexicon.
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE

A-fires: "Write the image gallery component lazy loading logic."
  → "write" fires IMPLEMENT. "component" fires UI. "logic" fires IMPLEMENT.
  → CompiledPredicateIds: ["UI", "IMPLEMENT"] → FLOW_EXECUTING

Teaching point: "write" always fires IMPLEMENT regardless of context.
"automate" is never in the IMPLEMENT lexicon.
