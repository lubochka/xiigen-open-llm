# Layer 2 — Nine-Node Flow Pairs (018)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: nine_node_flow (ImplementationFlow41)

## B15-nine-018 — SearchService (IMPLEMENT alone)

Request: "Build the search service that indexes products and returns ranked results for keyword queries."

### Flow: ImplementationFlow41

**Node 1 — DomainSplitterUnit**: Target: SearchService (feature_addition) | A = SelectedLogicalTransition: TransitionType: feature_addition, SubPhaseId: "4.1", PredicateIds: ["IMPLEMENT"] | Scope: product indexer, keyword query processor, result ranker, faceted filter engine.

**Node 2 — DomainTranslatorUnit**: Gate B: service must implement: (1) Indexing — build and update searchable product index; (2) Query Processing — parse keyword query, apply filters; (3) Result Ranking — score results by relevance (TF-IDF (Term Frequency-Inverse Document Frequency) or equivalent). Inject ISearchIndexStore interface. All methods return OperationResult<T>. First use of TF-IDF: Term Frequency-Inverse Document Frequency.

**Node 3 — SchemePlannerUnit**: probationary_accepted — scope valid; ISearchIndexStore abstraction approved; three work branches: Indexer, QueryProcessor, ResultRanker.

**Node 4 — DomainProcessorUnit**: WorkBranches processed:
- Indexer: ISearchService.IndexProduct(ProductDocument) → OperationResult<IndexConfirmation>; BR-SRCH-001: index update is idempotent (ProductId as document key).
- QueryProcessor: ISearchService.Search(SearchQuery) → OperationResult<SearchResultPage>; BR-SRCH-002: empty query returns empty result (no full-scan default).
- ResultRanker: internal scoring — TitleMatch > DescriptionMatch > TagMatch; BR-SRCH-003: results sorted descending by relevance score.

**Node 5 — ApplierUnit**: Files generated: ISearchService.cs, SearchService.cs, ISearchIndexStore.cs, SearchQuery.cs, SearchResultPage.cs, RelevanceRanker.cs, ProductDocument.cs.

**Node 6 — ResultTranslatorUnit**: 7 files, 2 interfaces, 3 business rules. Test surface: 10 recommended cases (exact match, partial match, multi-word, empty query, no results, ranking order).

**Node 7 — ArbiterSelectorUnit**: Arbiter: implementation-completeness-arbiter. ReviewFocus: three domains covered, ISearchIndexStore injected, empty-query guard present.

**Node 8 — ArbiterUnit**: Verdict: PASS. Three domains covered. ISearchIndexStore injected via constructor. Empty-query guard (BR-SRCH-002) confirmed.

**Node 9 — FormatterUnit**: Verdict: IMPLEMENT_APPROVED | TransitionType: feature_addition | Files: 7 | BusinessRules: 3 | TestSurface: 10 cases
