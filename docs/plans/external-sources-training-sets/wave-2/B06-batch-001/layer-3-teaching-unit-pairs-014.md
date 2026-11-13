# Layer 3 — Teaching Unit Pairs (014)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## Method note

File 014 covers InventoryManagement teaching — three pairs.

---

## B06-L3-014-001 — Teaching: "epics" and "capability" fire DOCUMENT

```
source_id: xiigen-b06-l3-014-001
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-INVENTORY-TEACH-001"
  Content:
    rules_demonstrated:
      "Rule 15": "epics fires DOCUMENT"
      "Rule 16": "capability fires DOCUMENT"
    inventory_examples:
      "specify the epics for inventory" → DOCUMENT (epics fires)
      "define capability requirements for inventory" → DOCUMENT (capability + requirements fire)
      "monitor inventory levels" → [] (monitor is not a predicate)

---

## B06-L3-014-002 — Teaching: inventory domain gaps

```
source_id: xiigen-b06-l3-014-002
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-INVENTORY-TEACH-002"
  Content:
    gap_vocabulary: ["track", "monitor", "reserve", "release", "manage", "optimize"]
    teaching_note: "None of these inventory operations fire predicates.
      To produce DOCUMENT, the request needs: define, describe, specify, document,
      functional, behavior, use-case, requirements, spec, criteria, capability, epics, etc."
    canonical_fix: "Change 'track inventory' to 'describe inventory tracking use cases' → DOCUMENT fires"

---

## B06-L3-014-003 — Teaching: InventoryManagement flow factory always selects 2.1

```
source_id: xiigen-b06-l3-014-003
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-INVENTORY-TEACH-003"
  Content:
    teaching_point: "Domain does not affect flow selection"
    rule: "SubPhaseId: 2.1 → RequirementsAuthoringFlow21 regardless of domain"
    inventory_case: "Inventory requirements → RequirementsAuthoringFlow21 (same as checkout, loyalty, tax, etc.)"
    anti_pattern: "Selecting a domain-specific flow like 'InventoryRequirementsFlow21' (does not exist)"
