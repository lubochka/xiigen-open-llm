# Layer 3 — Teaching Unit Pairs (016)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## Method note

File 016 covers ReturnAndRefund, TaxCalculation, MultiCurrency teaching — three pairs.

---

## B06-L3-016-001 — Teaching: "criteria" fires DOCUMENT

```
source_id: xiigen-b06-l3-016-001
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-RETURN-TAX-TEACH-001"
  Content:
    rule: "criteria fires DOCUMENT"
    examples:
      "define the criteria for return eligibility" → DOCUMENT (define + criteria fire)
      "specify functional criteria for tax calculation" → DOCUMENT (specify + functional + criteria fire)
      "document fraud scoring criteria" → DOCUMENT (document + criteria fire)
    teaching_note: "criteria is in the DOCUMENT lexicon. It fires whether singular (criterion) or plural."

---

## B06-L3-016-002 — Teaching: domain words do not fire predicates

```
source_id: xiigen-b06-l3-016-002
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-DOMAIN-NOUNS-TEACH-001"
  Content:
    rule: "Domain nouns never fire predicates alone"
    domain_nouns_tested:
      "refund" → no predicate
      "tax" → no predicate
      "currency" → no predicate
      "VAT (Value Added Tax)" → no predicate
      "discount" → no predicate
      "bundle" → no predicate
    complete_sentences:
      "refund processing" → [] (knowledge-edge)
      "define refund requirements" → ["DOCUMENT"] (define fires)
      "tax compliance at checkout" → [] (knowledge-edge)
      "document tax compliance behavior" → ["DOCUMENT"] (document + behavior fire)

---

## B06-L3-016-003 — Teaching: DOCUMENT+ARCHITECTURE → documentation (not feature_addition)

```
source_id: xiigen-b06-l3-016-003
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-ARCH-DOC-TEACH-001"
  Content:
    rule: "ARCHITECTURE alone does not upgrade TransitionType to feature_addition"
    cases:
      "document tax requirements and design architecture" → DOCUMENT+ARCHITECTURE → documentation
      "write tax requirements and design architecture" → DOCUMENT+ARCHITECTURE+IMPLEMENT → feature_addition
    key_distinction: "IMPLEMENT or UI presence → feature_addition. ARCHITECTURE alone → documentation."
    tax_example:
      request: "Document the tax requirements and design the architecture for the tax service"
      predicates: ["DOCUMENT", "ARCHITECTURE"]
      transition_type: "documentation"
      anti_pattern: "Setting feature_addition because architecture is mentioned"
