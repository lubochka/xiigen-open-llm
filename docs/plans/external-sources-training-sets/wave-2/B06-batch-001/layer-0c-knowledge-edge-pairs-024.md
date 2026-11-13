# Layer 0-C — Knowledge-Edge Pairs (024)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 024 is the knowledge-edge summary for files 013-024, consolidating all gap
patterns and boundary cases for the new domains.

---

## B06-0C-024-001 — Gap summary: files 013-024

```
source_id: xiigen-b06-0c-024-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-024-001-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    b06_gap_summary_013_to_024:
      "Gap pattern 1: project management verbs":
        verbs: ["plan", "manage", "schedule", "monitor", "track", "coordinate"]
        result: "KNOWLEDGE_EDGE (no predicate fires)"
        examples: ["plan the sprint", "manage the delivery", "monitor performance"]

      "Gap pattern 2: operational execution verbs":
        verbs: ["detect", "block", "alert", "comply", "convert", "apply", "calculate"]
        result: "KNOWLEDGE_EDGE (no predicate fires)"
        examples: ["detect fraud", "comply with GDPR", "convert currency"]

      "Gap pattern 3: domain nouns alone":
        nouns: ["shipping", "inventory", "loyalty", "tax", "fraud", "privacy"]
        result: "KNOWLEDGE_EDGE (nouns never fire predicates alone)"
        examples: ["tax compliance at checkout", "fraud at checkout"]

      "Boundary: partial predicate wins":
        rule: "If ANY predicate term is present, the result is NOT knowledge-edge"
        examples:
          - "'Plan the sprint for requirements implementation' → DOCUMENT fires on 'requirements'"
          - "'Manage privacy compliance requirements' → DOCUMENT fires on 'requirements'"

---

## B06-0C-024-002 — Gap vs spec vocabulary: complete reference table

```
source_id: xiigen-b06-0c-024-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-024-002-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap_vs_spec_table:
    "gap: plan X" vs "spec: define X requirements" → gap has no predicate; spec fires DOCUMENT
    "gap: track X" vs "spec: describe X behavior" → gap has no predicate; spec fires DOCUMENT
    "gap: monitor X" vs "spec: document X criteria" → gap has no predicate; spec fires DOCUMENT
    "gap: manage X" vs "spec: specify X specification" → gap has no predicate; spec fires DOCUMENT
    "gap: roadmap X" vs "spec: document X epics" → gap has no predicate; spec fires DOCUMENT
    "gap: detect X" vs "spec: define X requirements" → gap has no predicate; spec fires DOCUMENT
    "gap: comply with X" vs "spec: document X compliance requirements" → gap has no predicate; spec fires DOCUMENT

---

## B06-0C-024-003 — Teaching consolidation: all knowledge-edge cases in files 013-024

```
source_id: xiigen-b06-0c-024-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-024-003-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    total_gap_cases_013_to_024: 30+
    gap_domains_covered: 10
    gap_verb_types_covered: ["project management", "operational execution", "standalone domain nouns"]
    spec_predicates_confirmed: ["DOCUMENT alone", "DOCUMENT+IMPLEMENT"]
    boundary_cases:
      "partial predicate wins": 4 cases demonstrated (requirements in gap context)
      "domain noun alone = gap": confirmed across all 10 domains
      "DOCUMENT verb without requirements noun still fires": confirmed (describe, define, specify)
    teaching_note: "Knowledge-edge occurs ONLY when the entire request contains zero
      predicate-lexicon terms. A single DOCUMENT term anywhere in the sentence prevents
      knowledge-edge classification. The lexicon scan is full-text, not position-dependent."
