# Layer 3 — Teaching Unit Pairs (017)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## Method note

File 017 covers BundleAndDiscount, SubscriptionBilling, FraudDetection teaching — three pairs.

---

## B06-L3-017-001 — Teaching: "business" fires DOCUMENT (Rule 7)

```
source_id: xiigen-b06-l3-017-001
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-BUSINESS-TEACH-001"
  Content:
    rule: "Rule 7: business fires DOCUMENT"
    examples:
      "define the business rules for discount stacking" → DOCUMENT (define + business fire)
      "document the business rules for subscription billing" → DOCUMENT (document + business fire)
      "write the business requirements for fraud detection" → DOCUMENT+IMPLEMENT (business+requirements fire DOCUMENT; write fires IMPLEMENT)
    anti_pattern: "Treating 'business' as a non-predicate word — it IS in the DOCUMENT lexicon"

---

## B06-L3-017-002 — Teaching: "develop" fires IMPLEMENT (produces feature_addition)

```
source_id: xiigen-b06-l3-017-002
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-DEVELOP-TEACH-001"
  Content:
    rule: "develop is in IMPLEMENT lexicon — fires IMPLEMENT even in documentation context"
    examples:
      "develop the feature-brief for address book integration" → DOCUMENT+IMPLEMENT
        predicates: ["DOCUMENT", "IMPLEMENT"]
        DOCUMENT_source: "feature-brief"
        IMPLEMENT_source: "develop"
        transition_type: "feature_addition"
    anti_pattern: "Treating 'develop the feature-brief' as DOCUMENT only because object is documentation"
    correct: "Compiler is lexicon-based. 'develop' fires IMPLEMENT regardless of object."

---

## B06-L3-017-003 — Teaching: fraud and privacy are DOCUMENT domains (not separate predicates)

```
source_id: xiigen-b06-l3-017-003
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-FRAUD-PRIVACY-TEACH-001"
  Content:
    rule: "Security, fraud, and privacy vocabulary does not add predicates beyond DOCUMENT"
    examples:
      "define fraud detection requirements" → ["DOCUMENT"]
      "define privacy compliance requirements" → ["DOCUMENT"]
      "define security requirements" → ["DOCUMENT"]
    anti_pattern: "Adding a SECURITY predicate for fraud/privacy requests"
    correct: "No SECURITY predicate exists. Fraud/privacy are business requirement domains.
      DOCUMENT fires on 'define', 'requirements'. Security/fraud/privacy are domain nouns."
    routing_result: "All three examples route to 2.1. No separate security sub-phase."
