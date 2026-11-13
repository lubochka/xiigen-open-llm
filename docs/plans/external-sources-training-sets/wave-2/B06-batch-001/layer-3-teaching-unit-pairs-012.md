# Layer 3 — Teaching Unit Pairs (012)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Method note

File 012 covers the complete B06 teaching summary — all rules, all patterns, all corruption types in one reference.

---

## B06-L3-012-001 — Complete B06 teaching summary

```
source_id: xiigen-b06-l3-012-001
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "Complete B06 batch teaching summary — all rules in one reference"

B:
  TeachingUnit: "B06-COMPLETE-SUMMARY-001"
  Content:
    title: "Complete B06 Batch Teaching Summary"
    scenario: "B06 — Checkout flow business requirements and use cases"
    primary_predicate: "DOCUMENT"
    secondary_predicates: ["IMPLEMENT", "ARCHITECTURE", "UI"]

    document_lexicon_17_terms:
      - document, spec, specification, requirement, story, criteria
      - describe, define, functional, behavior, use-case, narrative
      - feature-brief, capability, epics, business, stakeholder

    key_rules_numbered:
      "Rule 1":  "Predicates are independent — never suppress one for another"
      "Rule 5":  "describe fires DOCUMENT (not IMPLEMENT)"
      "Rule 6":  "define fires DOCUMENT (not IMPLEMENT)"
      "Rule 7":  "business fires DOCUMENT"
      "Rule 8":  "stakeholder fires DOCUMENT AND triggers 2.4 routing"
      "Rule 9":  "IMPLEMENT or UI present → TransitionType: feature_addition"
      "Rule 13": "narrative fires DOCUMENT"
      "Rule 14": "feature-brief fires DOCUMENT (not feature alone)"
      "Rule 15": "epics fires DOCUMENT"
      "Rule 16": "capability fires DOCUMENT"
      "Rule 17": "DomainId is always software-development for B06"

    implement_verbs: ["write", "build", "create", "develop", "implement", "code"]

    routing_map:
      "DOCUMENT → 2.1": "RequirementsAuthoringFlow21 (always)"
      "DOCUMENT + stakeholder → 2.4": "StakeholderRequirementsFlow24 (conditional)"
      "IMPLEMENT → 4.1": "ImplementationFlow41"
      "ARCHITECTURE → 3.1": "ArchitectureFlow31"
      "UI → 1.1": "ScreenInventoryFlow11"

    transition_type_rule: "feature_addition if IMPLEMENT or UI present; otherwise documentation"

    v6_corruption_types:
      "v6-01": "DOCUMENT missed — lexicon term present but predicate not fired"
      "v6-02": "IMPLEMENT suppressed — action verb present but IMPLEMENT not fired"
      "v6-03": "Routing collapsed — multiple required sub-phase decisions reduced to fewer"

    canonical_ordering: "2.1 → 3.1 → 2.4 → 1.1 → 4.1"

---

## B06-L3-012-002 — B06 anti-patterns summary

```
source_id: xiigen-b06-l3-012-002
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-ANTI-PATTERNS-001"
  Content:
    title: "B06 Anti-Patterns — What NOT To Do"
    anti_patterns:
      AP-1:
        pattern: "Treating 'write user stories' as DOCUMENT only"
        correct: "DOCUMENT (story, criteria) + IMPLEMENT (write)"
        corruption_type: "v6-02"
      AP-2:
        pattern: "Treating 'feature' as a DOCUMENT term"
        correct: "'feature' alone → IMPLEMENT; 'feature-brief' → DOCUMENT"
        corruption_type: "v6-01"
      AP-3:
        pattern: "Routing 'stakeholder requirements' to 2.1 only"
        correct: "→ 2.1 AND 2.4 (stakeholder triggers both)"
        corruption_type: "v6-03"
      AP-4:
        pattern: "Setting TransitionType: documentation when IMPLEMENT is present"
        correct: "IMPLEMENT presence → feature_addition unconditionally"
        corruption_type: "v6-03"
      AP-5:
        pattern: "Treating 'define' as an IMPLEMENT verb"
        correct: "'define' is in DOCUMENT lexicon — fires DOCUMENT only"
        corruption_type: "v6-02"
      AP-6:
        pattern: "Setting DomainId to anything other than software-development"
        correct: "Rule 17 — always software-development for B06"
        corruption_type: "metadata error"
      AP-7:
        pattern: "Selecting only one flow when multiple predicates are active"
        correct: "Each predicate gets its own flow; flows run concurrently"
        corruption_type: "v6-03"

---

## B06-L3-012-003 — B06 batch file inventory

```
source_id: xiigen-b06-l3-012-003
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-FILE-INVENTORY-001"
  Content:
    title: "B06-batch-001 Complete File Inventory"
    total_files: 101
    layer_0a_compiler: ["pairs.md", "pairs-002.md through 012.md"] ; count: 12
    layer_0b_router: ["pairs.md", "pairs-002.md through 012.md"] ; count: 12
    layer_0c_knowledge_edge: ["pairs.md", "pairs-002.md through 012.md"] ; count: 12
    layer_0d_flow_factory: ["pairs.md", "pairs-002.md through 012.md"] ; count: 12
    layer_1_transition: ["pairs.md", "pairs-002.md through 012.md"] ; count: 12
    layer_2_nine_node: ["pairs.md", "pairs-002.md through 012.md"] ; count: 12
    layer_3_teaching: ["pairs.md", "pairs-002.md through 012.md"] ; count: 12
    held_out_corruption: ["pairs.md", "pairs-002.md through 012.md"] ; count: 12
    cross_layer_synthesis: ["001.md through 004.md"] ; count: 4
    manifest: ["MANIFEST.md"] ; count: 1
    total_check: 12*8 + 4 + 1 = 101
    teaching_coverage: "Complete — all B06 lexicon terms, rules, routing patterns, v6 corruptions, and metrics covered across 33 teaching units"
