# Layer 2 — Nine-Node Flow Pairs (016)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: nine_node_flow (ImplementationFlow41 + ArchitectureFlow31)

## B15-nine-016 — ProductCatalogService (IMPLEMENT+ARCHITECTURE)

Request: "Build the product catalog service and design the domain model for product categories and variants."

### Two flows: ArchitectureFlow31 then ImplementationFlow41

**Node 1 — DomainSplitterUnit**: Target: ProductCatalogService (feature_addition) | A = SelectedLogicalTransition: TransitionType: feature_addition, SubPhaseIds: ["3.1", "4.1"], PredicateIds: ["IMPLEMENT", "ARCHITECTURE"] | ArchitectureFlow31 runs first; ImplementationFlow41 consumes architecture output.

**Node 2 — DomainTranslatorUnit (Architecture)**: Gate B: domain model must define: Product aggregate (ProductId, Name, SKU, Category, Variants[]); Category entity (CategoryId, Name, ParentCategoryId for hierarchy); Variant value object (VariantId, AttributeKey, AttributeValue, PriceModifier). Bounded context: CatalogContext.

**Node 3 — SchemePlannerUnit (Architecture)**: probationary_accepted — architecture scope valid; CatalogContext boundary approved.

**Node 4 — DomainProcessorUnit (Architecture)**: Domain model artifacts: Product aggregate root, Category entity, Variant value object, CatalogContext boundary. ArchitectureFlow31 produces: catalog-domain-model-spec artifact passed to ImplementationFlow41.

**Node 5 — ApplierUnit (Architecture → Implementation)**: ImplementationFlow41 consumes domain model:
- ICatalogService.ListProducts(filter) → OperationResult<PagedResult<ProductSummary>>
- ICatalogService.GetProduct(ProductId) → OperationResult<ProductDetail>
- ICategoryService.GetCategoryTree() → OperationResult<CategoryNode[]>
- Files generated: ICatalogService.cs, ProductCatalogService.cs, ICategoryService.cs, CategoryService.cs, Product.cs, Category.cs, Variant.cs, CatalogRepository.cs.

**Node 6 — ResultTranslatorUnit**: 8 files, 3 interfaces, domain model with 3 entities, 1 aggregate root. Test surface: 15 recommended cases.

**Node 7 — ArbiterSelectorUnit**: Arbiter: architecture-completeness-arbiter then implementation-completeness-arbiter.

**Node 8 — ArbiterUnit**: Architecture verdict: PASS (domain model complete, bounded context defined). Implementation verdict: PASS (catalog, category, variant all implemented; fabric-first confirmed).

**Node 9 — FormatterUnit**: Verdict: IMPLEMENT_APPROVED (ARCHITECTURE+IMPLEMENT) | TransitionType: feature_addition | Files: 8 | DomainEntities: 3 | TestSurface: 15 cases
