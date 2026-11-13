# ARCH Pairs — Sub-phase 3.2 Data Model and Persistence Design
# Batch: dpo-pairs-arch-plan-batch-001
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md
# Sub-phase scope: entity shape, storage access patterns, consistency rules, persistence constraints
# Pair count: 5

---

## Pair AP1-3.2-001 — entity shape decision confused with repository implementation

```
source_id: xiigen-arch-plan-batch-001-AP1-3.2-001
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 3.2
rejected_sub_phase: 4.1
```

Prompt: `"design the data model for a feature flag system that supports per-tenant overrides and gradual rollout"`

```
CHOSEN — 3.2 Data Model and Persistence Design:
Two entity shapes, one consistency rule, one forbidden coupling.

FeatureFlag (root):
  flagKey (primary key, immutable string).
  defaultValue (boolean).
  rolloutBucketSize (int, 0-100).
  createdAtUtc (timestamp).
  No tenant column on this entity. The default is global.

TenantFlagOverride:
  Composite primary key: (tenantId, flagKey).
  enabled (boolean).
  reason (free-text, required).
  createdAtUtc.
  Foreign key reference to FeatureFlag.flagKey. Cascade-delete: when a flag
  is removed, all its overrides are removed atomically.

Consistency rule: an evaluation request always reads the most recent
TenantFlagOverride for (tenantId, flagKey) within strong consistency. If no
override exists, fall back to the FeatureFlag.defaultValue with rollout
bucket evaluation against tenantId. Eventual consistency is not acceptable
for overrides because a failed override read silently returns the default,
which is the failure mode the override was added to prevent.

Forbidden coupling: TenantFlagOverride must not store the userId who created
the override. That information lives in an audit table, not in the operational
entity. Mixing audit metadata into operational rows blocks future GDPR delete
operations and inflates row size on the hot path.

Index plan: primary key index on TenantFlagOverride is sufficient for
evaluation reads. No global index across tenants — flag evaluation is
always scoped to a single tenant.

REJECTED — 4.1 Backend Services:
Create a FeatureFlagRepository class with GetFlag(flagKey),
GetTenantOverride(tenantId, flagKey), and SetTenantOverride(tenantId,
flagKey, enabled, reason) methods. Use Entity Framework Core with the
generic IRepository<T> pattern. Inject DbContext through constructor.
Apply a global query filter on TenantFlagOverride to enforce tenant scoping.
[WRONG: "design the data model" asks for entity shape, primary keys,
cardinality, consistency level, and forbidden columns. Writing the repository
class skips all of those decisions and assumes them implicitly through the
DbContext mappings. A repository written before the model is decided commonly
encodes the wrong assumptions — for example, including a userId column on
operational rows because the repository signature requires it, even though
the audit-vs-operational separation forbids it.]
```

---

## Pair AP1-3.2-002 — partitioning decision confused with adapter implementation

```
source_id: xiigen-arch-plan-batch-001-AP1-3.2-002
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 3.2
rejected_sub_phase: 4.5
```

Prompt: `"the events table has 200 million rows and queries are slow — should we partition it, and if so by what?"`

```
CHOSEN — 3.2 Data Model and Persistence Design:
Yes. Partition by month on event_timestamp.

Justification: 95% of operational queries select events from a single
month-window (analytics dashboards default to current month, retention sweeps
target months older than the retention window). A partition key on
event_timestamp prunes 95% of partitions on those queries before the
storage layer sees them. Partition by tenantId is rejected because heavy
tenants would create skewed partitions and hot-spot one storage node.

Partition design:
  Range partition: events_yyyy_mm where each partition holds one calendar month.
  Constraint: every event_timestamp insert must fall in an existing partition.
  Pre-create partitions one month ahead in a scheduled job; never let an insert
  hit a missing partition (this would silently land in the default partition
  and break retention sweeps).

Retention rule: events older than 13 months are dropped by detaching and
removing the oldest monthly partition. Partition detach is a constant-time
operation; row-by-row delete on a 200M-row table is not.

Forbidden: queries that span more than 12 partitions are not optimized by
this design and must be rewritten to scope to a smaller window or to a
read-replica analytics pipeline.

Consistency: writes are serializable within a partition. Cross-partition
reads are eventually consistent because storage commits one partition at
a time during multi-month inserts (rare in practice).

REJECTED — 4.5 Integration and Provider Connectors:
Add an EventsArchiveAdapter that calls the EventStorageProvider API to fetch
historical events asynchronously. Implement Polly retry with exponential
backoff for transient failures. Cache recent results in Redis with a 5-minute
TTL. Update the EventsController to fall through to the archive adapter when
the primary query takes longer than 2 seconds. Configure the adapter to use
the streaming endpoint for queries that span more than one month.
[WRONG: "should we partition it, and if so by what" is a storage-level decision
about how rows are organized in the database. The adapter approach changes the
runtime read path without changing the underlying storage — the table still
has 200M rows in one heap, the slow queries are still slow, and a new caching
layer is added to mask the symptom. The partition decision must be made first;
if reads are still slow after correct partitioning, then an adapter and cache
become a follow-on consideration.]
```

---

## Pair AP1-3.2-003 — storage boundary question confused with adapter implementation

```
source_id: xiigen-arch-plan-batch-001-AP1-3.2-003
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 3.2
rejected_sub_phase: 4.5
```

Prompt: `"the inventory service and the order service both query the products table — should they share the schema or each have their own?"`

```
CHOSEN — 3.2 Data Model and Persistence Design:
Each service owns its own schema. They share an immutable productId reference,
not a relational table.

Inventory service owns:
  inventory_items (productId, quantityOnHand, reservedQuantity,
  reorderPoint, lastReceivedAtUtc, location).
  productId is a string identifier — not a foreign key — to a product
  that exists in the catalog service.

Order service owns:
  order_lines (orderId, productId, quantityOrdered, priceAtOrderTime,
  productNameSnapshot, productSkuSnapshot).
  Critical rule: the order line stores a snapshot of product name and SKU at
  order time. It does not join against the catalog at read time. A product
  rename a year after the order must not retroactively change the order line.

Forbidden: inventory must not query order_lines to compute reserved quantity.
Reserved quantity is owned by inventory and updated through OrderPlaced and
OrderCancelled events. Joining across schemas reintroduces the coupling that
this design prevents.

Why not share: a shared products table forces all schema changes to coordinate
across both services. A column addition by the catalog team breaks the
inventory deployment until inventory adopts the new schema. The string
productId reference is the contract; the schema is private to each service.

Trade-off: the snapshot duplication uses more storage. This is intentional —
historical accuracy is the goal, and storage is cheap relative to the
operational cost of cross-service schema coordination.

REJECTED — 4.5 Integration and Provider Connectors:
Add a ProductCatalogAdapter to the inventory service that calls the catalog
service REST API to look up product details when needed. Add the same
ProductCatalogAdapter to the order service. Implement IDistributedCache
caching on the adapter with a 1-hour TTL keyed by productId. Add Polly
retry policy with three attempts on transient failures. Register the
adapter as scoped in both services.
[WRONG: "should they share the schema" is a persistence-boundary decision
about whether two services own one table or two tables and how cross-service
references work. Adding an adapter that calls the catalog service is the
implementation of "they have separate schemas" — but only after that boundary
decision is made. The adapter alone does not answer the question; it presupposes
an answer (separate schemas with runtime joins) that may not be the right one
(the chosen response argues for snapshots, not runtime joins).]
```

---

## Pair AP1-3.2-004 — consistency rule decision confused with repository implementation

```
source_id: xiigen-arch-plan-batch-001-AP1-3.2-004
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 3.2
rejected_sub_phase: 4.1
```

Prompt: `"design the consistency model for the shopping cart — adds and removes happen from web and mobile concurrently"`

```
CHOSEN — 3.2 Data Model and Persistence Design:
Last-write-wins on a per-line basis with a conflict-resolution rule for the
quantity field. Cart-level locking is rejected.

Cart entity shape:
  cartId (root identifier, owned by user session).
  userId.
  lines: list of CartLine.
  cartVersion (monotonic int, incremented on every write).
  lastModifiedAtUtc.

CartLine:
  productId (line identity within the cart).
  quantity.
  addedAtUtc.
  lastModifiedAtUtc.

Conflict rules:
  Different lines: writes commute. Web adding lineA while mobile adds lineB
  produces a cart with both lines.
  Same line, different operations: a SetQuantity operation always wins over
  an AddToCart operation if their writes interleave at the database. Reasoning:
  the user explicitly chose a quantity; the implicit increment from a second
  AddToCart click is more likely to be a duplicate than an intent.
  Same line, same operation type: last-write-wins by lastModifiedAtUtc.

cartVersion is for observability, not for optimistic concurrency. Optimistic
concurrency over the whole cart blocks legitimate concurrent adds from
different lines and produces user-visible "your cart is out of date" errors
that are not actionable.

Consistency level: per-line writes are serializable within a single
write. The cart as a whole is eventually consistent across web and mobile
clients with a maximum convergence window of 500ms (the event propagation
budget).

Forbidden: a SELECT FOR UPDATE on the entire cart row during an add. This
would block concurrent adds from another device and produce user-visible
delays.

REJECTED — 4.1 Backend Services:
Create a CartRepository class with AddLine, RemoveLine, and SetQuantity
methods. Use EF Core with a RowVersion column for optimistic concurrency
control. Wrap each method in a transaction with READ COMMITTED isolation.
On a DbUpdateConcurrencyException, retry up to three times with a small
delay. Inject IDbContextFactory through the constructor. Add unit tests
for the optimistic concurrency retry path.
[WRONG: "design the consistency model" asks for the conflict resolution
rules, the consistency level for each operation, and what is forbidden.
Writing the repository with optimistic concurrency picks one strategy
(reject conflicting writes) and locks it in before the rule has been
decided — and the chosen strategy is the one explicitly rejected by the
correct design (optimistic concurrency on the whole cart blocks legitimate
concurrent adds and produces non-actionable errors). The model decision
must be made first.]
```

---

## Pair AP1-3.2-005 — entity shape question confused with repository implementation

```
source_id: xiigen-arch-plan-batch-001-AP1-3.2-005
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 3.2
rejected_sub_phase: 4.1
```

Prompt: `"design the data model for storing user notification preferences — channels (email/push/sms) and per-category opt-in/opt-out"`

```
CHOSEN — 3.2 Data Model and Persistence Design:
Two entities. The default state is opt-in for all categories on all channels;
overrides are stored only when the user diverges from the default.

UserNotificationPreference:
  Composite key: (userId, category, channel).
  enabled (boolean).
  changedAtUtc (timestamp, required).
  changeReason enum: USER_OPT_IN, USER_OPT_OUT, ADMIN_FORCE_OFF, COMPLIANCE_BLOCK.
  Only rows where enabled diverges from the default are stored.

NotificationCategory (lookup, immutable at runtime):
  category (primary key).
  defaultChannel set (which channels are default-on for this category).
  isLegallyRequired (boolean — if true, USER_OPT_OUT is forbidden;
  COMPLIANCE_BLOCK is the only enabled=false reason permitted).

Cardinality: a user with default preferences has zero rows in
UserNotificationPreference. Storing only diverging rows means a deletion
of the user has at most O(categories * channels) rows to remove, and a
new user creates zero rows on signup.

Forbidden: the UserNotificationPreference table must not store the IP
address of the request that made the change. That belongs in an audit log,
not in the operational table that determines whether to deliver a message.

Compliance rule: when a category has isLegallyRequired = true, a write with
changeReason = USER_OPT_OUT must be rejected at the persistence layer (CHECK
constraint). The check at the persistence layer is the last line of defence;
service-layer validation is not enough because admin scripts and migrations
can bypass it.

REJECTED — 4.1 Backend Services:
Create a NotificationPreferencesRepository with GetByUser(userId),
SetPreference(userId, category, channel, enabled), and DeleteAllForUser(userId)
methods. Use a junction table UserPreferences with columns userId, categoryId,
channelId, enabled, and a unique index on the three-column key. Apply EF Core
auto-include for the User and Category navigation properties. Validate
isLegallyRequired in the service layer before saving.
[WRONG: "design the data model" asks for entity shape, default-vs-override
storage strategy, cardinality, and what must be enforced at the persistence
layer (not the service layer). Service-layer validation alone for legally
required notifications is the failure mode the chosen design prevents — admin
scripts and migrations bypass service code. The CHECK constraint is part of
the model, not part of the repository class.]
```
