# MANIFEST — B15-batch-001
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 (001–012, synthesis 001–004) | Extended: 2026-05-06 (013–024, synthesis 005–008)
# Total files: 201

---

## File Count by Layer

| Layer | Files |
|-------|-------|
| Layer 0-A (compiler pairs) | 24 (001–024) |
| Layer 0-B (router pairs) | 24 (001–024) |
| Layer 0-C (knowledge edge pairs) | 24 (001–024) |
| Layer 0-D (flow factory pairs) | 24 (001–024) |
| Layer 1 (transition orchestrator pairs) | 24 (001–024) |
| Layer 2 (nine-node 4.1 pairs) | 24 (001–024) |
| Layer 3 (teaching unit pairs) | 24 (001–024) |
| Held-out and corruption pairs | 24 (001–024) |
| Cross-layer synthesis | 8 (001–008) |
| MANIFEST | 1 |
| **Total** | **201** |

---

## Layer 2 Nine-Node 4.1 Pairs

| File | Service | Verdict |
|------|---------|---------|
| 001 | Original batch — order processing service | — |
| 002 | Customer account service | IMPLEMENT_APPROVED |
| 003 | Inventory tracking service | IMPLEMENT_APPROVED |
| 004 | Order cancellation service | IMPLEMENT_APPROVED |
| 005 | Payment reconciliation service | IMPLEMENT_APPROVED |
| 006 | Shipping label generator | IMPLEMENT_APPROVED |
| 007 | Order event publisher (probationary_rejected then fallback) | IMPLEMENT_APPROVED (fallback) |
| 008 | Price calculation engine | IMPLEMENT_APPROVED |
| 009 | Nightly batch processor (500K scale) | IMPLEMENT_APPROVED |
| 010 | Idempotent order submission handler | IMPLEMENT_APPROVED |
| 011 | Refund processing service | IMPLEMENT_APPROVED |
| 012 | Order export service (refactoring) | IMPLEMENT_APPROVED (refactoring) |
| 013 | OrderFulfillmentService — IMPLEMENT alone | IMPLEMENT_APPROVED |
| 014 | RefundAndChargebackService — IMPLEMENT alone | IMPLEMENT_APPROVED |
| 015 | InventoryAllocationService — IMPLEMENT alone | IMPLEMENT_APPROVED |
| 016 | ProductCatalogService — IMPLEMENT+ARCHITECTURE | IMPLEMENT_APPROVED |
| 017 | ReviewAndRatingService — IMPLEMENT alone | IMPLEMENT_APPROVED |
| 018 | SearchService — IMPLEMENT alone | IMPLEMENT_APPROVED |
| 019 | NotificationService — IMPLEMENT+API_CONTRACT | IMPLEMENT_APPROVED |
| 020 | AuditService — IMPLEMENT+SECURITY | IMPLEMENT_APPROVED |
| 021 | ReportingService — IMPLEMENT+DOCUMENT | IMPLEMENT_APPROVED |
| 022 | PricingEngineRefactor — IMPLEMENT (refactoring) | IMPLEMENT_APPROVED (refactoring) |
| 023 | ProbationaryRejected — IMPLEMENT+QA (fallback used) | IMPLEMENT_APPROVED (via fallback) |
| 024 | Complete nine-node rule summary for 4.1 | — (reference) |

---

## Key Teaching Points

1. **IMPLEMENT trigger verbs** — build/implement/create/scaffold/bootstrap/add(code artifact)/write(code)/extend/refactor all fire IMPLEMENT
2. **"contract" fires API_CONTRACT** — "data contracts", "service contracts", "API contract" — modifier doesn't matter; "contract" keyword fires API_CONTRACT
3. **"domain model" fires ARCHITECTURE** — even embedded in IMPLEMENT requests; "bounded context" also fires ARCHITECTURE
4. **"scaffold" and "bootstrap" are IMPLEMENT** — project initialization verbs = IMPLEMENT same as build/create
5. **Four predicates never collapse** — each action verb fires its own predicate independently; deduplication only for same predicate from multiple triggers
6. **probationary_accepted / probationary_rejected** — Gate B feasibility check in nine-node flow; fallback scope used on rejection
7. **TransitionType: refactoring overrides feature_addition** — "refactor" verb present → refactoring, regardless of other verbs
8. **QA always last** — absolute ordering constraint in all multi-predicate pipelines
9. **No GapHandler in B15** — all requests fire at least IMPLEMENT; no capability gaps in this batch
10. **OperationResult<T> everywhere** — every public method returns OperationResult<T>; no exceptions for business logic
11. **Fabric-first pattern** — inject interfaces, not concrete types; BaseService(ISessionStorage) for session-persistent services
12. **v6 corruptions** — v6-01: FULFILLMENT/PAYMENT/ORDER_PROCESSING invented; v6-02: "data contracts" or "domain model" not caught; v6-03: multi-predicate collapsed

---

## Cross-Layer Synthesis Traces

| File | Scenario | Predicate Set |
|------|----------|---------------|
| 001 | Build order service + integrate Stripe | IMPLEMENT+API_CONTRACT |
| 002 | Build fulfillment service with domain model | IMPLEMENT+ARCHITECTURE |
| 003 | Discount engine + tests + docs + QA | IMPLEMENT+TEST+DOCUMENT+QA |
| 004 | Full checkout service pipeline | ARCHITECTURE+API_CONTRACT+IMPLEMENT+SECURITY+TEST+DOCUMENT+QA |
| 005 | OrderFulfillmentService — IMPLEMENT alone trace | IMPLEMENT |
| 006 | ProductCatalogService — IMPLEMENT+ARCHITECTURE with domain model | IMPLEMENT+ARCHITECTURE |
| 007 | InventoryAllocationService — IMPLEMENT+API_CONTRACT+SECURITY+TEST+DOCUMENT four-predicate trace | IMPLEMENT+API_CONTRACT+DOCUMENT+SECURITY+TEST |
| 008 | TaxService — full six-predicate trace | ARCHITECTURE+API_CONTRACT+IMPLEMENT+SECURITY+TEST+DOCUMENT |
