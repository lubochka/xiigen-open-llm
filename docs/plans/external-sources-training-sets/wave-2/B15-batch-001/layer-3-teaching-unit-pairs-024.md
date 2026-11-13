# Layer 3 — Teaching Unit Pairs (024)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: teaching_unit

## TU-024-A — Complete B15 predicate rule summary (013-024 extension)

New services added (013-024):
- OrderFulfillmentService (013): IMPLEMENT; +API_CONTRACT on integrate; +ARCHITECTURE on domain model
- RefundAndChargebackService (014): IMPLEMENT; +API_CONTRACT on wire up; +ARCHITECTURE on domain model; +TEST on write tests
- InventoryAllocationService (015): IMPLEMENT; +API_CONTRACT on data contracts; +ARCHITECTURE on bounded context; +SECURITY on harden
- ProductCatalogService (016): IMPLEMENT; +ARCHITECTURE on domain model or architecture; four predicates for full pipeline
- ReviewAndRatingService (017): IMPLEMENT; +SECURITY on secure (not on moderate); +TEST on write tests; +DOCUMENT on spec out
- SearchService (018): IMPLEMENT; +API_CONTRACT on integrate; refactoring on refactor verb; extend → feature_addition
- NotificationService (019): IMPLEMENT; +API_CONTRACT on integrate/connect; deduplication applies across multiple API triggers
- AuditService (020): IMPLEMENT; +SECURITY on secure/harden; append-only alone does not fire SECURITY
- ReportingService (021): IMPLEMENT; +DOCUMENT on document/spec out; service contracts → API_CONTRACT; DOCUMENT parallel-eligible with IMPLEMENT
- PricingEngineService (022): IMPLEMENT; refactor → TransitionType: refactoring; strategy pattern does not fire ARCHITECTURE
- TaxService (023): IMPLEMENT; +ARCHITECTURE on architect/domain model/tax model; architect verb fires ARCHITECTURE directly
- Rule reference (024): this file

## TU-024-B — Cross-service OperationResult<T> confirmation

Every public method across all 012 new services returns OperationResult<T>:
- OperationResult<T>.Ok(value) — success path
- OperationResult<T>.Fail(reason, "ERROR_CODE") — failure path
Never: throw for business logic. Never: return null. Never: return bool without context.

## TU-024-C — Cross-service fabric-first confirmation

Every service injects its dependencies as interfaces:
- ICarrierClient (FulfillmentService), IPaymentService (RefundService), IInventoryRepository (AllocationService)
- ICatalogRepository (CatalogService), IPurchaseHistoryService (ReviewService), ISearchIndexStore (SearchService)
- IEmailClient + ISmsClient (NotificationService), IHmacSigner (AuditService), IReportDataSource (ReportingService)
None are newed up inside service constructors. All registered in DI (Dependency Injection) container.

## TU-024-D — Deduplication rule confirmed across services

Multiple triggers for same predicate in one request → one predicate → one routing decision → one factory call → one flow.
Examples confirmed: three API verbs → one API_CONTRACT (NotificationService); two IMPLEMENT verbs → one IMPLEMENT (any service); two DOCUMENT verbs → one DOCUMENT (ReportingService).

## TU-024-E — Complete v6 corruption risk summary for 013-024 extension

v6-01 risks: "chargeback" suppresses IMPLEMENT; "fulfillment" suppresses IMPLEMENT; "jurisdiction" suppresses IMPLEMENT.
v6-02 risks: "bounded context" not caught as ARCHITECTURE (015); "catalog architecture" not caught (016); "tax model" not caught (023); "packing model" not caught (013).
v6-03 risks: IMPLEMENT+API_CONTRACT collapsed to one decision (018 integrate+Elasticsearch); IMPLEMENT+SECURITY collapsed (020); four-predicate collapsed to one (016 full catalog pipeline).
