# Layer 2 — Nine-Node 3.3 Pairs (017)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: nine_node

---

## B12-L2-017 — ProductSearchGraphQL: API_CONTRACT alone (documentation)

```
source_id: xiigen-b12-l2-017
flow_name: ProductSearchGraphQL
transition_type: documentation
phase: 3
sub_phase: 3.3
pair_kind: nine_node
gate_b: PASS
```

### Input (N01)

N01 classification:
  user_request: "Specify the GraphQL query contract for the product search API including filter and sort parameters."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  DomainId: "software-development"
  FlowId: "ProductSearchGraphQL"
  SubPhase: "3.3"
  SelectedLogicalTransition: A = SelectedLogicalTransition
  TriggerTerms:
    API_CONTRACT: "GraphQL", "query contract", "API"
    DOCUMENT: "specify"

### Processing (N02-N08)

N02 — DomainTranslatorUnit:
  Domain: product catalog, search functionality
  API style: GraphQL
  Operations: product search with filters, sort, pagination

N03 — SchemePlannerUnit:
  WorkBranches:
    - branch-001: searchProducts query type
    - branch-002: ProductResult and FacetGroup types
    - branch-003: filter and sort input types

N04 — DomainProcessorUnit:
  Query: searchProducts(query: String!, filters: [FilterInput], sort: SortInput, page: PageInput): SearchResult
  SearchResult: { products: [ProductResult], facets: [FacetGroup], totalCount: Int, pageInfo: PageInfo }
  ProductResult: { id: ID!, sku: String!, name: String!, price: Float!, availability: String, imageUrl: String }
  FacetGroup: { name: String!, values: [FacetValue!]! }
  FilterInput: { field: String!, values: [String!]!, operator: FilterOperator }
  SortInput: { field: String!, direction: SortDirection }
  PageInput: { limit: Int, offset: Int }

N05 — ApplierUnit:
  Apply: product-search.graphql (schema definition language file)
  Apply: query contract documentation

N06 — ResultTranslatorUnit:
  types_defined: 7 (SearchResult, ProductResult, FacetGroup, FacetValue, FilterInput, SortInput, PageInput)
  queries_defined: 1 (searchProducts)
  pagination: offset-based

N07 — ArbiterSelectorUnit:
  Request: api-contract-completeness-arbiter

N08 — ArbiterUnit:
  Verdict: PASS
  Findings: "searchProducts query defined with filters/sort/pagination; 7 types defined; all fields typed"

### Output (N09)

N09 — FormatterUnit:
  FlowStatus: "PASS"
  Artifacts:
    1. product-search.graphql (GraphQL schema definition)
    2. product-search-graphql-guide.md (from DocumentationFlow21)
  GateB: PASS — "product-search.graphql defines searchProducts query with ≥4 input/output types"
  TransitionType: "documentation"
