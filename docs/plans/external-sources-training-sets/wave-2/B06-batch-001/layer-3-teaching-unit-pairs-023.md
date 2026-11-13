# Layer 3 — Teaching Unit Pairs (023)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## Method note

File 023 covers complete anti-pattern sweep for files 013-024 — three pairs.

---

## B06-L3-023-001 — Teaching: anti-patterns for new domains (complete list)

```
source_id: xiigen-b06-l3-023-001
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-ANTI-PATTERNS-013-024"
  Content:
    title: "B06 Anti-Patterns for Files 013-024"
    anti_patterns:
      AP-N1:
        pattern: "Adding domain-specific predicates (SECURITY, PRIVACY, LOGISTICS)"
        correct: "Only DOCUMENT, IMPLEMENT, ARCHITECTURE, UI predicates exist"
      AP-N2:
        pattern: "Treating fraud/privacy/security as separate sub-phases"
        correct: "All route to 2.1 (Business Requirements) via DOCUMENT predicate"
      AP-N3:
        pattern: "Using domain nouns as predicate sources (tax, refund, subscription)"
        correct: "Only the 17 DOCUMENT lexicon terms fire DOCUMENT"
      AP-N4:
        pattern: "Setting DomainId to business domain (finance, security, logistics)"
        correct: "DomainId: software-development always (Rule 17)"
      AP-N5:
        pattern: "Selecting domain-specific flows (TaxFlow, FraudFlow, PrivacyFlow)"
        correct: "RequirementsAuthoringFlow21 for all DOCUMENT 2.1 requirements work"
      AP-N6:
        pattern: "Creating new sub-phases for new domains (2.5, 2.6)"
        correct: "Sub-phase map is fixed. New domains fit within existing sub-phases."

---

## B06-L3-023-002 — Teaching: complete rule citations for files 013-024

```
source_id: xiigen-b06-l3-023-002
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-RULE-CITATIONS-013-024"
  Content:
    rules_reinforced:
      Rule_7: "business fires DOCUMENT"
      Rule_8: "stakeholder fires DOCUMENT + triggers 2.4 routing"
      Rule_9: "IMPLEMENT or UI → feature_addition"
      Rule_13: "narrative fires DOCUMENT"
      Rule_14: "feature-brief fires DOCUMENT"
      Rule_15: "epics fires DOCUMENT"
      Rule_16: "capability fires DOCUMENT"
      Rule_17: "DomainId: software-development always"
      Dual_predicate: "write/create/build/develop + DOCUMENT noun → [DOCUMENT, IMPLEMENT]"
      Knowledge_edge: "zero predicate terms = knowledge-edge"
      Partial_predicate: "one predicate term = not knowledge-edge"
      v6_corruptions: "v6-01 (DOCUMENT missed), v6-02 (IMPLEMENT suppressed), v6-03 (routing collapsed)"

---

## B06-L3-023-003 — Teaching: PairParser and ModelTrainer metrics for 013-024

```
source_id: xiigen-b06-l3-023-003
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-PARSER-METRICS-013-024"
  Content:
    PairParserUnit_metrics:
      files_parsed: 96
      pairs_per_layer:
        layer_0a: "12 files × 5 pairs = 60"
        layer_0b: "12 files × 4 pairs = 48"
        layer_0c: "12 files × 3 pairs = 36"
        layer_0d: "12 files × 3 pairs = 36"
        layer_1:  "12 files × 4 pairs = 48"
        layer_2:  "12 files × 4 nodes = 48"
        layer_3:  "12 files × 3 pairs = 36"
        held_out: "12 files × 3 pairs = 36"
      total_pairs_added: 348
    ModelTrainerUnit_metrics:
      new_domains_covered: 10
      new_teaching_points_added: 30+
      v6_corruption_types_reinforced: "all 3 (v6-01, v6-02, v6-03)"
