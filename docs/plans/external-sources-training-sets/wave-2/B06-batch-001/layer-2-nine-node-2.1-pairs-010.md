# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 2.1 (010)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## Method note

File 010 covers SearchAndFilterSpec: probationary_rejected candidate flow — fallback used.
The nine-node flow completes via fallback. N01 through N09.

---

## B06-2.1-010-N01 — Node 01 DomainSplitterUnit: SearchAndFilterSpec (fallback flow)

```
source_id: xiigen-b06-2.1-010-n01
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
transition_type: documentation
phase: 2
sub_phase: 2.1
node_role: node-01-domain-splitter
scenario: SearchAndFilterSpec
trust_state: deterministic_generic
flow_note: probationary_rejected candidate; fallback universal flow used
pair_kind: nine_node_flow
```

A:
  TransitionId: "transition-B06-search-filter-spec-2.1"
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  UserRequest: "Specify the functional requirements for the product search and filter capabilities in the checkout flow — faceted search, keyword search, and price range filtering."
  PredicateIds: ["DOCUMENT"]
  FlowSelectionNote: "probationary_rejected: candidate flow search-filter-flow-2.1-candidate had 6 of 9 nodes; fallback universal flow selected"

B:
  DomainPackets:
    - PacketId: "B06-2.1-010-packet-document"
      DomainTag: "DOCUMENT"
      Focus: "Search and filter functional requirements — faceted search by category/brand/size, keyword search, price range filter, sort options, and checkout context filter behavior"
      UserSignal: "functional requirements, search, filter, checkout flow"
  PacketCount: 1
  Confidence: 0.92

---

## B06-2.1-010-N02 — Node 02 DomainTranslatorUnit: search/filter requirements

```
source_id: xiigen-b06-2.1-010-n02
contract_signature: DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
node_role: node-02-domain-translator
scenario: SearchAndFilterSpec
pair_kind: nine_node_flow
```

B:
  StatementId: "B06-2.1-010-stmt-document"
  TechnicalStatement: "Search and filter functional requirements: (1) Keyword Search: full-text search across product name, description, and tags; minimum 2 characters; results ranked by relevance; (2) Faceted Search: filter by category (hierarchical, 3 levels), brand, size, color, availability (in-stock only option); multiple facets selectable simultaneously; active facets shown as removable chips; (3) Price Range Filter: min/max price input; drag-range slider; currency-aware; (4) Sort Options: relevance (default), price low-high, price high-low, newest arrivals, best seller, average rating; (5) Checkout Context Filter: within checkout, search is scoped to items eligible for current promotional codes; out-of-stock items shown with 'notify me' only. Business rules: BR-SF01 (keyword min 2 chars); BR-SF02 (faceted multi-select); BR-SF03 (checkout scope filter for promo eligibility)."
  KeyTerms: ["faceted search", "keyword search", "price range filter", "sort options", "availability filter"]
  Confidence: 0.92

---

## B06-2.1-010-N03 — Node 03 SchemePlannerUnit: search/filter branches

```
source_id: xiigen-b06-2.1-010-n03
contract_signature: SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
node_role: node-03-scheme-planner
scenario: SearchAndFilterSpec
pair_kind: nine_node_flow
```

B:
  WorkBranches:
    - BranchId: "B06-2.1-010-branch-keyword"    ; Scope: "Keyword search requirements" ; Priority: 1
    - BranchId: "B06-2.1-010-branch-faceted"    ; Scope: "Faceted search requirements" ; Priority: 2
    - BranchId: "B06-2.1-010-branch-price"      ; Scope: "Price range filter"          ; Priority: 3
    - BranchId: "B06-2.1-010-branch-sort"       ; Scope: "Sort options"                ; Priority: 4
    - BranchId: "B06-2.1-010-branch-checkout"   ; Scope: "Checkout context filter"     ; Priority: 5
    - BranchId: "B06-2.1-010-branch-rules"      ; Scope: "3 business rules"            ; Priority: 6

  BranchCount: 6
  Confidence: 0.92

---

## B06-2.1-010-N09 — Node 09 FormatterUnit: SearchAndFilterSpec complete (fallback flow)

```
source_id: xiigen-b06-2.1-010-n09
contract_signature: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
node_role: node-09-formatter
scenario: SearchAndFilterSpec
trust_state_note: fallback flow used (probationary_rejected)
pair_kind: nine_node_flow
```

B:
  FlowId: "universal-nine-node-development-flow-2.1"
  OverallVerdict: "PASS"
  FinalHumanResult: |
    Search and filter functional specification (sub-phase 2.1):
    [Produced by fallback universal flow — candidate flow rejected]

    Keyword Search: full-text across name, description, tags; min 2 chars; relevance ranked. BR-SF01.
    Faceted Search: category (3-level), brand, size, color, availability; multi-select; active
      chips removable. BR-SF02.
    Price Range Filter: min/max input + drag slider; currency-aware.
    Sort Options: relevance, price asc/desc, newest, best seller, rating.
    Checkout Context Filter: promo-eligible items scoped; out-of-stock with "notify me". BR-SF03.

    3 business rules (BR-SF01 through BR-SF03).
    Flow note: universal fallback used — candidate search-filter-flow-2.1-candidate was
      probationary_rejected (6 of 9 nodes covered).
    Arbiter reviews: PASS (5 search/filter types complete), PASS (rule testability).

  Metrics:
    search_type_count: "5"
    business_rule_count: "3"
    flow_used: "universal-nine-node-development-flow-2.1 (fallback)"
    candidate_rejected: "search-filter-flow-2.1-candidate (6/9 nodes)"
  Warnings:
    - "fallback_flow_used: candidate probationary_rejected"
