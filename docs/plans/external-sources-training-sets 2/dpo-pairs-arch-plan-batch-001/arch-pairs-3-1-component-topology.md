# ARCH Pairs — Sub-phase 3.1 Architecture and Component Topology
# Batch: dpo-pairs-arch-plan-batch-001
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md
# Sub-phase scope: component boundaries, dependency direction, lifecycle topology, system responsibilities
# Pair count: 6

---

## Pair AP1-3.1-001 — capability ownership confused with class implementation

```
source_id: xiigen-arch-plan-batch-001-AP1-3.1-001
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 3.1
rejected_sub_phase: 4.1
```

Prompt: `"who owns scheduling, who owns notification delivery, and who owns the retry policy in the new alert pipeline?"`

```
CHOSEN — 3.1 Architecture and Component Topology:
Three components own this capability with a strict dependency direction.

Scheduler — owns when an alert fires.
  State: cron schedule, next-fire-time index. Stateless beyond the schedule store.
  Allowed dependencies: TimeSource only.
  Forbidden: Scheduler must not call DeliveryDispatcher directly.
    It emits AlertDue events; DeliveryDispatcher subscribes.

DeliveryDispatcher — owns sending one alert.
  State: per-attempt status, last_attempted_at, attempt_count.
  Allowed dependencies: TemplateStore (read-only), DeliveryProvider (outbound).
  Forbidden: must not own retry policy decisions.

RetryPolicyEvaluator — owns whether to retry and when.
  Pure function over (failure_reason, attempt_count, last_attempted_at).
  No I/O, no storage of its own.
  Allowed dependencies: none.
  Returns NextAttemptAt or RetryExhausted.

Dependency direction: Scheduler -> AlertDue event -> DeliveryDispatcher
-> RetryPolicyEvaluator (call). No reverse calls. RetryPolicyEvaluator
returning RetryExhausted is the terminal signal — DeliveryDispatcher then
emits AlertFailed.

Forbidden coupling: Scheduler must not import RetryPolicyEvaluator.
A retry decision is a delivery-time concern, not a scheduling concern.

REJECTED — 4.1 Backend Services:
Implement the AlertPipelineService class with three public methods:
ScheduleAlert(scheduleId, payload), DispatchAlert(alertId), and EvaluateRetry(alertId).
Inject IScheduleStore, ITemplateStore, IDeliveryProvider, and ILogger through
the constructor. Register the service as scoped in DI. Add a private
RetryWithBackoff helper that wraps DispatchAlert in a Polly policy with
exponential backoff up to five attempts.
[WRONG: "who owns" is an ownership and dependency-direction question —
which component is responsible for each decision and which calls are forbidden.
A single AlertPipelineService that holds all three concerns is the architectural
mistake the prompt is trying to prevent. Boundary decisions must be made before
any class is written; otherwise the class collapses three separate
responsibilities into one and reproduces the original coupling problem.]
```

---

## Pair AP1-3.1-002 — boundary redesign confused with class extraction

```
source_id: xiigen-arch-plan-batch-001-AP1-3.1-002
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 3.1
rejected_sub_phase: 4.4
```

Prompt: `"the reporting module touches user data, billing data, and audit data — redesign the boundaries so it stops being the place that knows everything"`

```
CHOSEN — 3.1 Architecture and Component Topology:
The reporting module must stop being the integration point. Three separate
components replace it, each owning its own data access path.

UserReportingProjection — owns user-facing reports.
  Reads: a UserReportView read model populated by domain events from the
  user service. Forbidden: direct queries against the user service database.

BillingReportingProjection — owns billing-facing reports.
  Reads: a BillingReportView read model populated by domain events from the
  billing service. Forbidden: direct joins against billing tables.

AuditReportingProjection — owns audit-facing reports.
  Reads: the append-only audit log. Forbidden: writes of any kind to user or
  billing data; audit reports are read-only by definition.

Rule: each projection owns its own read model storage. No projection joins
across read models. If a report needs cross-domain data, the projection
subscribes to the additional event stream and adds the needed shape to its
own read model.

Consequence: each domain service can deploy independently. Reporting is no
longer the bottleneck and no longer the place that knows everyone's schema.

REJECTED — 4.4 Refactoring and Code Quality:
Extract the user-related queries from ReportingService into UserReportQueries.
Extract billing-related queries into BillingReportQueries. Extract audit-related
queries into AuditReportQueries. Update the DI registrations to wire the new
classes. Inject IUserDbContext, IBillingDbContext, and IAuditDbContext into
the appropriate query class. Move the existing SQL into the new files.
[WRONG: "redesign the boundaries" is a structural ownership decision — which
component owns which data, what is forbidden, and how cross-domain reports
are produced without joining. Extracting query classes that still call into
all three database contexts reproduces the original coupling — the reporting
code still knows everyone's schema, just spread across three files. Refactoring
moves code; it does not change ownership.]
```

---

## Pair AP1-3.1-003 — service ownership question confused with class implementation

```
source_id: xiigen-arch-plan-batch-001-AP1-3.1-003
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 3.1
rejected_sub_phase: 4.1
```

Prompt: `"should rate limiting live in the API gateway or in each service — define the boundary"`

```
CHOSEN — 3.1 Architecture and Component Topology:
Rate limiting belongs at the API gateway for cross-cutting limits and in
each service for domain-specific limits. The boundary is purpose, not technology.

API Gateway owns:
  Per-tenant request budgets — global concurrency caps and request-per-minute
  limits per tenant id. Reasoning: applies uniformly to every endpoint regardless
  of which service handles the request. Cannot be expressed without a global
  view of all in-flight requests.

Each service owns:
  Domain-specific limits — for example, "no more than five password reset
  attempts per email in 15 minutes" lives inside the auth service because it
  needs auth-domain state (account lock status, prior attempt history) that the
  gateway does not have and must not have.

Forbidden: services must not enforce per-tenant request budgets. The gateway
must not enforce password reset attempt limits. Crossing this boundary forces
the gateway to know auth domain state, which is exactly the coupling the
boundary prevents.

Consequence: a denial-of-service flood is rejected at the gateway before any
service is touched. A password-reset enumeration attack is rejected at the
auth service because it requires reading auth-specific state.

REJECTED — 4.1 Backend Services:
Add a RateLimitingMiddleware to the gateway pipeline using IDistributedCache
for the token bucket store. Inject the middleware before AuthenticationMiddleware.
For password reset throttling, add a PasswordResetThrottle service in the auth
project that wraps the existing PasswordResetController with a Polly bulkhead
policy. Register both in the DI container as singletons.
[WRONG: "define the boundary" asks where rate limiting decisions belong and
what each side is forbidden to know. Writing the middleware classes presupposes
a boundary decision that has not been made. Without the boundary rule, the
middleware will end up duplicating logic across the gateway and the services
or moving auth state into the gateway — both of which violate the ownership
rule the prompt is trying to establish.]
```

---

## Pair AP1-3.1-004 — dependency direction question confused with class implementation

```
source_id: xiigen-arch-plan-batch-001-AP1-3.1-004
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 3.1
rejected_sub_phase: 4.1
```

Prompt: `"the recommendation engine and the catalog service have ended up calling each other — fix the dependency direction"`

```
CHOSEN — 3.1 Architecture and Component Topology:
Catalog is the source of truth for product data. Recommendation is a derived
view over that data. The dependency direction must be one-way: Recommendation
depends on Catalog; Catalog must not call Recommendation.

Catalog Service:
  Owns product entities, categories, prices, availability.
  Publishes ProductCreated, ProductUpdated, ProductRemoved domain events.
  Forbidden: must not call any recommendation endpoint or query any
  recommendation read model.

Recommendation Engine:
  Subscribes to Catalog events and maintains its own read-optimized index.
  May call Catalog read endpoints for fresh data on demand.
  Forbidden: must not be a dependency of Catalog. Catalog must operate
  correctly when Recommendation is offline.

The reverse call exists today because Catalog imports a "PopularityScore"
helper from Recommendation to sort search results. That import must be deleted.
Catalog instead exposes a sort_by_popularity flag whose ordering is computed
locally from cached aggregate counts. Fresh popularity data is updated by
events from Recommendation back to Catalog through an event channel — this
keeps the dependency direction at compile time even when data flows both ways
at runtime.

Compile-time rule: the Catalog project must have no reference to the
Recommendation project. Enforce in CI.

REJECTED — 4.1 Backend Services:
Refactor CatalogService to remove the direct call to RecommendationClient.
Move the RecommendationClient injection out of the CatalogService constructor.
Add a new SortingService that wraps both ProductRepository and
RecommendationRepository, exposing GetSortedProducts(sortBy). Update the API
controllers to call SortingService instead of CatalogService for sort
requests. Register SortingService as scoped.
[WRONG: "fix the dependency direction" is a boundary decision about what
each service is allowed to know about and call. Adding a SortingService that
holds references to both repositories preserves the bidirectional dependency
just hidden behind a new layer. The rule "Catalog must not depend on
Recommendation" is a project-reference rule enforced at compile time; a new
class that imports both does not satisfy it.]
```

---

## Pair AP1-3.1-005 — system responsibility question confused with class implementation

```
source_id: xiigen-arch-plan-batch-001-AP1-3.1-005
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 3.1
rejected_sub_phase: 4.1
```

Prompt: `"who is responsible for invalidating the cache when a product price changes — the price service, the cache, or the API layer?"`

```
CHOSEN — 3.1 Architecture and Component Topology:
The price service owns invalidation. The cache and the API layer are passive
in this question. Push the invalidation signal from the source of change.

Price Service:
  When a price write commits, the service publishes a PriceChanged domain event
  containing productId, old price, new price, and effective_at timestamp.
  This is the only authoritative signal. The price service does not call the
  cache or the API layer directly.

Cache Layer:
  Subscribes to PriceChanged events and evicts the cache key for the affected
  productId. Cache eviction is a pure consequence of the event; the cache
  contains no knowledge of what business action caused the change.

API Layer:
  Subscribes to PriceChanged events for any in-flight cached responses tied
  to the affected product (CDN-edge invalidation). The API layer never
  initiates invalidation.

Rule: the cache must never poll the price service. Polling reverses the
dependency and creates two failure modes — stale reads when polling is slow,
and tight coupling when the cache imports the price service client.

The trade-off accepted: an event-bus outage means cache stale reads continue
until the bus recovers. This is preferable to coupling the cache to the price
service or letting the API layer orchestrate invalidation across multiple
caches.

REJECTED — 4.1 Backend Services:
Implement a PriceUpdateService.UpdatePrice(productId, newPrice) method that
opens a transaction, writes the new price to the database, then calls
ICacheClient.InvalidateAsync(productId) and IApiCacheClient.PurgeAsync(productId)
within the same transaction. Register all three clients as scoped dependencies
on PriceUpdateService. Add retry logic for the cache invalidation calls so
that a transient cache failure does not roll back the price write.
[WRONG: "who is responsible" is an ownership and dependency-direction question.
Wiring direct calls from PriceUpdateService to the cache and to the API layer
gives the price service two new compile-time dependencies and three failure
modes (DB success / cache fail / API fail), which the question explicitly tries
to avoid. The boundary decision — push via events, never call directly — must
be made before any service code is written.]
```

---

## Pair AP1-3.1-006 — boundary rule question confused with refactoring task

```
source_id: xiigen-arch-plan-batch-001-AP1-3.1-006
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 3.1
rejected_sub_phase: 4.4
```

Prompt: `"we keep finding helper code that knows about three different domains — what rule should we add to stop this from happening?"`

```
CHOSEN — 3.1 Architecture and Component Topology:
Add a single rule and enforce it at the project-reference level.

The rule: shared helper projects may depend on at most one domain project.
A helper that needs types from two domain projects is the wrong abstraction —
it is either an integration that belongs at a domain boundary or a duplicated
utility that should be split.

Apply at three layers:
  Compile-time: each helper csproj declares the one domain it serves. CI
  rejects any helper that adds a second domain reference. The rule is a
  build artifact, not documentation.
  Code-review: any pull request that adds a "common helper for X and Y"
  type is rejected at review and reshaped into either a domain-specific
  helper plus an integration component, or two distinct utilities.
  Runtime: the orchestrator's dependency-direction check fails the build
  if a helper assembly references types from more than one domain assembly.

Consequence: cross-domain helper code stops accumulating because there is
nowhere it can live. Either the helper is split, or the work moves into a
named integration component whose existence makes the cross-domain coupling
visible.

This rule does not make existing cross-domain helpers compile. The rule's
purpose is to fail the build until those helpers are split. The migration
is intentional and visible.

REJECTED — 4.4 Refactoring and Code Quality:
Extract the cross-domain helpers into a Common.Utilities project. Move the
shared functions into a SharedDomainHelpers static class. Update all call sites
to reference the new project. Add unit tests for each helper. Document the
helpers in the project README. Run the build and confirm no compilation errors
before merging.
[WRONG: "what rule should we add" asks for a boundary policy that prevents
the situation from recurring. Extracting the existing helpers into a Common
project does not stop new cross-domain helpers from being added — it just
relocates the current ones. Without a rule that forbids the pattern, the
Common project becomes the next dumping ground. The decision is a policy,
not a refactoring task.]
```
