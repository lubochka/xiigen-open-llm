# Layer 0-C — Knowledge-Edge Pairs (036)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## Method note

File 036 is the knowledge-edge summary for files 025-036, consolidating all gap
patterns and boundary cases for Round 2 domains.

---

## B06-0C-036-001 — Gap summary: files 025-036

```
source_id: xiigen-b06-0c-036-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-036-001-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    b06_gap_summary_025_to_036:
      "Gap pattern 1: operational execution verbs":
        verbs: ["process", "validate", "apply", "check", "handle", "redeem", "track", "assign",
                "monitor", "calculate", "route", "split", "fallback", "notify", "trigger",
                "send", "schedule", "show", "display", "filter", "rank", "cache", "refresh",
                "activate", "expire", "enforce", "coordinate", "generate", "revoke", "throttle"]
        result: "KNOWLEDGE_EDGE (no predicate fires)"
      "Gap pattern 2: domain nouns alone":
        nouns: ["gift card", "redemption", "referral", "invite", "flash sale", "countdown",
                "bundle", "SKU", "wholesale", "dropship", "abandoned cart", "recommendation",
                "cross-sell", "upsell", "seasonal campaign", "digital delivery", "download",
                "license key", "CDN token"]
        result: "KNOWLEDGE_EDGE (nouns never fire predicates alone)"
      "Boundary: partial predicate wins":
        rule: "If ANY predicate term is present, result is NOT knowledge-edge"
        examples:
          - "'Track business referrals' → DOCUMENT fires on 'business' (Rule 7)"
          - "'Activate flash sale requirements' → DOCUMENT fires on 'requirements'"
          - "'Negotiate wholesale pricing epics' → DOCUMENT fires on 'epics' (Rule 15)"
          - "'Generate CDN architecture' → ARCHITECTURE fires on 'architecture' (Rule A-02)"

---

## B06-0C-036-002 — Gap vs spec vocabulary: Round 2 complete reference

```
source_id: xiigen-b06-0c-036-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-036-002-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  complete_gap_vs_spec_reference_025_to_036:
    GiftCardRedemption: "process/validate/apply gift card" → gap; "define/specify requirements" → spec
    ReferralProgram: "track/assign/monitor referrals" → gap; "define/document requirements" → spec
    FlashSaleEngine: "run/activate/reserve flash sale" → gap; "describe/specify requirements" → spec
    BundlePricing: "calculate/apply bundle discounts" → gap; "define/document rules" → spec
    WholesaleDiscount: "calculate/assign/negotiate wholesale" → gap; "describe/specify tiers" → spec
    DropshippingRules: "route/split/fallback orders" → gap; "define/document rules" → spec
    AbandonedCartRecovery: "send/trigger/schedule emails" → gap; "specify/define requirements" → spec
    ProductRecommendation: "show/filter/rank products" → gap; "define/describe requirements" → spec
    CrossSellUpsell: "show/display/trigger upsell" → gap; "define/specify rules" → spec
    SeasonalPromotion: "activate/schedule/expire campaigns" → gap; "describe/define rules" → spec
    DigitalProductDelivery: "email/generate/revoke/expire" → gap; "define/specify requirements" → spec

---

## B06-0C-036-003 — Teaching consolidation: all knowledge-edge cases in files 025-036

```
source_id: xiigen-b06-0c-036-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-036-003-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    total_gap_cases_025_to_036: 55+
    gap_domains_covered: 11 (Round 2)
    gap_verb_types_covered: ["operational execution", "scheduling/activation", "enforcement",
      "calculation", "display/presentation", "routing/assignment", "notification", "generation"]
    spec_predicates_confirmed: ["DOCUMENT alone", "DOCUMENT+IMPLEMENT", "DOCUMENT+ARCHITECTURE",
      "ARCHITECTURE alone", "DOCUMENT+ARCHITECTURE+IMPLEMENT"]
    boundary_cases:
      "partial predicate wins": confirmed across all 11 Round 2 domains
      "domain noun alone = gap": confirmed for all Round 2 vocabulary
      "DOCUMENT lexicon fires regardless of operational verb context": confirmed
      "ARCHITECTURE lexicon also prevents knowledge-edge": confirmed (file 035-003)
    teaching_note: "Knowledge-edge classification requires the ENTIRE request to contain
      zero predicate-lexicon terms from ANY predicate type (DOCUMENT, IMPLEMENT, ARCHITECTURE, UI).
      A single term from any lexicon prevents knowledge-edge."
