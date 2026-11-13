# Layer 0-C — Knowledge-Edge Pairs (045)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-045-001 — VirtualList: "virtual" gap confirmed

```
source_id: xiigen-b01-0c-045-001
source_license_state: first_party
pair_kind: knowledge_edge
```

Gap term "virtual" behavior:
  "virtual list" → "virtual" does NOT fire UI; "list" does NOT fire UI.
  "virtual rendering" → neither term fires any predicate.
  "virtual scroll" → neither term fires any predicate.
  "virtual DOM" → neither term fires any predicate.

Exception: "virtual" co-occurring with UI term:
  "build a virtual list component" → "component" fires UI; "virtual" and "list" do not contribute.
  "create virtual scroll widget" → "widget" fires UI; "virtual" does not contribute.

Rule: "virtual" is a domain modifier with no predicate lexicon mapping.

---

## B01-0C-045-002 — ARCHITECTURE sub-phase assignment: 3.1 vs 4.1 clarification

```
source_id: xiigen-b01-0c-045-002
source_license_state: first_party
pair_kind: knowledge_edge
```

Sub-phase assignment clarification for ARCHITECTURE predicate:
  ARCHITECTURE alone → SubPhaseId: "3.1"
  ARCHITECTURE + IMPLEMENT → SubPhaseIds: ["3.1", "4.1"] (two decisions)
  ARCHITECTURE + UI → SubPhaseIds: ["1.1", "3.1"] (two decisions)

Common confusion: ARCHITECTURE sometimes listed as "4.1" in older files — corrected.
Authoritative sub-phase mapping for B01-batch-001:
  UI → 1.1
  DOCUMENT → 2.1
  ARCHITECTURE → 3.1
  IMPLEMENT → 4.1
  SECURITY → SEC.1
  TEST → TEST.1
