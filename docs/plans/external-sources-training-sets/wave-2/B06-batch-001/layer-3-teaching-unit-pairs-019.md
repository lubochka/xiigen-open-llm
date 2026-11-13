# Layer 3 — Teaching Unit Pairs (019)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## Method note

File 019 covers multi-domain teaching consolidation — three pairs.

---

## B06-L3-019-001 — Teaching: all DOCUMENT verbs for new domains

```
source_id: xiigen-b06-l3-019-001
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-DOCUMENT-VERBS-013-024"
  Content:
    document_verbs_complete:
      fires_DOCUMENT_only: ["define", "describe", "document", "specify"]
      fires_DOCUMENT_plus_IMPLEMENT: ["write", "create", "build", "develop"]
      note: "These are the VERB patterns. The 17 DOCUMENT nouns/adjectives are a separate list."
    new_domain_verb_examples:
      "define shipping requirements" → ["DOCUMENT"]
      "write shipping spec" → ["DOCUMENT", "IMPLEMENT"]
      "describe loyalty behavior" → ["DOCUMENT"]
      "create loyalty feature-brief" → ["DOCUMENT", "IMPLEMENT"]
      "document tax narrative" → ["DOCUMENT"]
      "build tax specification" → ["DOCUMENT", "IMPLEMENT"]

---

## B06-L3-019-002 — Teaching: ten new domains → same rules apply

```
source_id: xiigen-b06-l3-019-002
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-TEN-DOMAINS-TEACH"
  Content:
    teaching_point: "The B06 predicate rules apply uniformly to all ten new domains"
    domains_added_in_013_to_024:
      - ShippingRequirements (file 013)
      - InventoryManagement (file 014)
      - LoyaltyProgram (file 015)
      - ReturnAndRefund (file 016)
      - TaxCalculation (file 017)
      - MultiCurrency (file 018)
      - BundleAndDiscount (file 019)
      - SubscriptionBilling (file 020)
      - FraudDetection (file 021)
      - DataPrivacy (file 022)
    uniform_rules:
      predicate_lexicon: "same 17 DOCUMENT terms for all domains"
      routing: "DOCUMENT → 2.1 for all domains"
      flow_selection: "RequirementsAuthoringFlow21 for all domains"
      domain_id: "software-development for all domains"

---

## B06-L3-019-003 — Teaching: gap vocabulary by domain (reference table)

```
source_id: xiigen-b06-l3-019-003
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-GAP-VOCAB-REFERENCE"
  Content:
    gap_verbs_by_domain:
      shipping: plan, manage, track, route, ship, deliver
      inventory: track, monitor, reserve, release, manage
      loyalty: promote, accrue, redeem, tier
      return_refund: handle, process, issue
      tax: audit, comply, calculate, estimate
      currency: convert, display, format, round
      bundle_discount: configure, apply, calculate, stack
      subscription: schedule, cancel, renew, charge
      fraud: detect, block, alert, investigate
      privacy: comply, delete, anonymize, mask
    teaching_note: "None of these fire predicates. A request with only these verbs
      and domain nouns produces [] (empty predicate set) and is classified as knowledge-edge."
