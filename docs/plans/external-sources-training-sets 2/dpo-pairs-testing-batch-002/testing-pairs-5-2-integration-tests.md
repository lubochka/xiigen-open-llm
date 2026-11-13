# Testing Pairs — Sub-phase 5.2 Integration Tests
# Batch: dpo-pairs-testing-batch-002
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md
# Sub-phase scope: integration test design, cross-component validation, database integration tests, fixture construction
# Pair count: 5

---

## Pair AP2-5.2-001 — real-database integration test confused with mocked unit test

```
source_id: xiigen-testing-batch-002-AP2-5.2-001
source_license_state: first_party
pair_kind: dpo_test
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 5.2
rejected_sub_phase: 5.1
```

Prompt: `"design integration tests that verify cross-tenant row-level security — tenant A must not see tenant B's data through any query path"`

```
CHOSEN — 5.2 Integration Tests:
Tests run against a real database container. Mocks are forbidden because the
property under test is the database's actual filter behaviour against real SQL.

Container fixture:
  Testcontainers spins up SQL Server with the production schema and the row-
  level security policies enabled. The fixture is shared across test classes
  in the collection (per-class containers waste 30s per class on startup).

Seed data:
  Three tenants (tenantA, tenantB, tenantC), 50 rows each in the Orders
  table, 100 rows each in the OrderLines table. Tenant ids are generated
  per test run so parallel runs do not collide.

Test catalogue:

Direct query path:
  T1 — connect as tenantA, SELECT * FROM Orders → only tenantA rows
        returned. Assert exact count = 50.
  T2 — connect as tenantA, SELECT * FROM OrderLines → only tenantA
        rows. The OrderLines table joins to Orders via orderId; the
        RLS policy must apply at the OrderLines level too, not only
        through the join.

ORM path:
  T3 — connect as tenantA via EF Core DbContext (which sets the
        SESSION_CONTEXT key), call _orders.ToList() → 50 rows.
  T4 — same connection, call _orders.Where(o => o.Id == knownTenantBOrderId)
        .ToList() → 0 rows. The where clause must not bypass RLS.

Bypass attempt paths:
  T5 — raw SQL through DbContext.Database.ExecuteSqlRaw with
        "SELECT * FROM Orders WHERE TenantId = @other" → 0 rows.
        RLS is enforced regardless of the where clause.
  T6 — connect through a stored procedure that runs as caller →
        only the caller's tenant's rows.
  T7 — connect through a stored procedure that runs as definer (a
        configuration mistake to detect) → assertion fails this case
        because the procedure was misconfigured; the test should fail
        the build.

Reset between tests:
  Respawn drops and re-seeds the data between tests so that one test's
  inserts cannot leak into another's count assertions.

Assertion shape:
  Equal expected counts on every query. Never "any rows returned" — that
  cannot prove zero leakage.

REJECTED — 5.1 Unit Test Design:
Write unit tests against a mocked DbContext. Use Moq to set up
_dbContext.Setup(x => x.Orders).Returns(<filtered list>) where the filtered
list is the test's expectation. For each test, configure the mock to return
only the current tenant's orders. Assert that the service returns those
orders. Add a test where the mock returns another tenant's orders to
verify the service rejects them.
[WRONG: cross-tenant row-level security depends on the database's actual
filter behaviour — RLS policies, SESSION_CONTEXT, the EF Core query
translation, and any raw-SQL paths. A mocked DbContext returns whatever
the test configures it to return; the test passes by definition. The mock
proves the test wrote the configuration correctly, not that the database
enforces isolation. Real-container integration tests are the only way to
verify the actual filter behaviour. Mocks for tenant isolation produce
green tests while the system leaks tenant data in production.]
```

---

## Pair AP2-5.2-002 — message-bus integration test confused with mocked unit test

```
source_id: xiigen-testing-batch-002-AP2-5.2-002
source_license_state: first_party
pair_kind: dpo_test
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 5.2
rejected_sub_phase: 5.1
```

Prompt: `"design integration tests for the order-placed event flow — publisher emits, subscribers receive in the right order, and dead-letter routing works"`

```
CHOSEN — 5.2 Integration Tests:
Real RabbitMQ broker via Testcontainers. The behaviour under test is the
broker's actual delivery semantics — ordering, redelivery, and dead-letter
routing — which mocks cannot reproduce.

Container fixture:
  Testcontainers spins up RabbitMQ 3.13 with the production exchange
  topology applied. Shared across the test collection.

Test publisher and subscribers:
  An OrderPublishedSpy connects as a real consumer to the order-placed
  exchange. A separate InventoryReservationSpy connects to the inventory-
  related queue. Both record received messages with timestamps for ordering
  assertions.

Test catalogue:

Single publish, single subscriber:
  T1 — publish OrderPlaced{orderId=O1} → InventoryReservationSpy receives
        within 500 ms. Assert message body equals the published body
        byte-for-byte.

Ordering across multiple publishes:
  T2 — publish OrderPlaced{O1}, OrderPlaced{O2}, OrderPlaced{O3} from a
        single publisher → InventoryReservationSpy receives them in
        that order. The assertion uses the spy's recorded timestamps.
  T3 — publish OrderPlaced{O1}, OrderPlaced{O2} from two different
        publishers concurrently → both messages received, but no
        ordering guarantee across publishers (the broker contract).

Redelivery on consumer failure:
  T4 — InventoryReservationSpy nacks the first delivery → the message is
        redelivered with the redelivered flag set. Assert that the body
        is identical and that redelivered=true.
  T5 — InventoryReservationSpy nacks 5 times in a row → the message
        moves to the dead-letter queue after the configured retry count.
        Assert that the dead-letter spy receives the message.

Dead-letter rule:
  T6 — a message in the dead-letter queue carries the x-death header
        with reason="rejected". Assert the header is present.

Idempotency:
  T7 — publish OrderPlaced{O1} twice with the same idempotency key →
        InventoryReservationSpy applies the reservation once. Assert
        the inventory state increments by exactly one reservation, not
        two.

REJECTED — 5.1 Unit Test Design:
Write unit tests against a mocked IEventBus. Use Moq to set up
_eventBus.Setup(x => x.Publish(It.IsAny<OrderPlaced>())).Returns(Task.CompletedTask).
For each test, call the service that emits the event, then verify
_eventBus.Verify(x => x.Publish(It.Is<OrderPlaced>(e => e.OrderId == "O1"))).
For ordering tests, use Moq's Sequence to verify the expected order.
For redelivery, mock the bus to throw on first call and succeed on second.
[WRONG: the behaviour under test is the broker's delivery semantics —
ordering across publishers, redelivery with the redelivered flag,
dead-letter routing with x-death headers. Mocks verify that the publisher
called Publish with the right arguments; they do not verify that the broker
delivers in order, that nack triggers redelivery, or that dead-letter
routing applies. The bug class these tests catch (broker misconfiguration,
wrong exchange binding, missing dead-letter declaration) is not visible to
a mock-based test. Integration tests with a real broker are the only way to
verify these properties.]
```

---

## Pair AP2-5.2-003 — provider-integration test confused with mocked unit test

```
source_id: xiigen-testing-batch-002-AP2-5.2-003
source_license_state: first_party
pair_kind: dpo_test
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 5.2
rejected_sub_phase: 5.1
```

Prompt: `"design integration tests for the Stripe payment integration — successful charge, declined card, idempotency, and webhook delivery"`

```
CHOSEN — 5.2 Integration Tests:
Tests run against Stripe's test environment using the Stripe-published test
card numbers. The behaviour under test is the integration with the real
Stripe API surface and the real webhook flow.

Configuration:
  Tests use the Stripe test API key (sk_test_...). No production keys.
  Tests use the Stripe-published test cards (4242... for success,
  4000000000000002 for decline, etc.) per the Stripe docs.

Test catalogue:

Successful charge:
  T1 — charge $10 against test card 4242... → Stripe returns succeeded
        status. Assert charge_id is non-empty and matches the format
        ch_<base58>. Assert the amount and currency on the response.

Declined card:
  T2 — charge $10 against test card 4000000000000002 (generic decline) →
        Stripe returns failed status with code "card_declined". Assert
        the failure_code matches the documented value.
  T3 — charge $10 against test card 4000000000009995 (insufficient funds)
        → Stripe returns failed status with code "card_declined" and
        decline_code "insufficient_funds".

Idempotency:
  T4 — call charge() twice with the same Idempotency-Key → second call
        returns the same charge object (the Stripe contract). Assert
        charge_id is identical between the two responses.
  T5 — call charge() twice with different idempotency keys → two
        distinct charges. Assert two different charge_ids.

Webhook delivery:
  T6 — trigger a charge.succeeded event using stripe trigger
        charge.succeeded (Stripe CLI). The local listener (stripe listen
        --forward-to localhost:5000/stripe-webhook) forwards to the
        running test app. Assert the test app's webhook handler logged
        the event.
  T7 — webhook signature verification: send a payload with a wrong
        signature → handler returns 400 and does not log the event.

Cleanup:
  Test charges are made against the Stripe test account; they have no real
  monetary effect. No cleanup needed.

REJECTED — 5.1 Unit Test Design:
Write unit tests against a mocked IStripeClient. Use Moq to set up
_stripeClient.Setup(x => x.Charge(It.IsAny<ChargeRequest>())).Returns(
new ChargeResponse { Status = "succeeded", ChargeId = "ch_test_123" }).
For decline tests, configure the mock to return a failed response. For
idempotency, configure the mock to return the same response on repeated
calls with the same key. For webhook tests, mock the webhook handler and
assert the expected event is processed.
[WRONG: the behaviour under test is the integration with Stripe's API
surface — the real shape of decline codes, the idempotency contract, the
webhook signature verification. Mocks return whatever the test sets up;
they do not catch a mismatch between our adapter's expected response shape
and Stripe's actual response shape. The bug class these tests catch
(Stripe added a new field, our adapter parses the wrong path, a webhook
signature header changed name) is not visible to a mock. Real integration
against the test environment is the only way to verify the contract.]
```

---

## Pair AP2-5.2-004 — integration test confused with end-to-end suite

```
source_id: xiigen-testing-batch-002-AP2-5.2-004
source_license_state: first_party
pair_kind: dpo_test
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 5.2
rejected_sub_phase: 5.3
```

Prompt: `"design integration tests for the report-export pipeline — the request/queue/worker/storage chain — without bringing up the React frontend"`

```
CHOSEN — 5.2 Integration Tests:
Tests exercise the back-end chain only. The frontend is out of scope; the
tests issue requests directly to the API, observe the queue, drive the worker,
and inspect storage.

Container fixture:
  Testcontainers brings up: PostgreSQL (the database), RabbitMQ (the queue),
  MinIO (S3-compatible object storage). The test process hosts the API
  server, the worker process, and the test client in one solution but as
  separate hosted services to mirror production-like wiring.

Seed data:
  A test tenant with 1000 rows of report-source data is seeded into
  PostgreSQL before each test class.

Test catalogue:

Happy path:
  T1 — POST /reports/export with body {format: "csv", filter: {...}} →
        201 with reportId. Wait up to 10 seconds for the worker to process
        the queue. GET /reports/{reportId} → status="completed",
        downloadUrl pointing to the MinIO bucket. Fetch the URL and
        assert the CSV content has 1000 rows + header.

Queue propagation:
  T2 — observe the RabbitMQ queue between request and completion: assert
        the message body contains the reportId, format, and filter.

Worker failure handling:
  T3 — configure the worker to throw on the first attempt → message
        is redelivered, second attempt succeeds. GET /reports/{reportId}
        eventually shows status="completed".
  T4 — worker throws 5 times → message moves to DLQ; GET /reports
        shows status="failed" with reason="processing_failed".

Format variants:
  T5 — POST with format=json → output in JSON with the same row count.
  T6 — POST with format=parquet → output in parquet; assert the file
        is parquet-readable by the test process.

Concurrent requests:
  T7 — submit 10 export requests for the same tenant concurrently →
        all 10 complete; the worker pool processes them in parallel up
        to its configured worker count.

Cleanup:
  Respawn resets PostgreSQL between tests. The MinIO bucket is cleared
  between test classes.

REJECTED — 5.3 End-to-End Scenario Testing:
Write a Playwright suite that signs in to the application, navigates to
the Reports page, fills in the export form, clicks Export, and waits for
the download. Use the browser's download event to capture the file. Assert
that the downloaded file contains the expected rows. Add tests for each
format option through the UI dropdown. Run the suite as part of the nightly
end-to-end pipeline.
[WRONG: the prompt explicitly excludes the frontend ("without bringing up
the React frontend"). Driving the chain through Playwright adds the React
app, the build step, the dev server, and many UI failure modes that are
not the property under test. Integration tests focus on the back-end chain;
end-to-end tests are the next sub-phase up and would test the full user
journey. Mixing the two scopes produces tests that fail for UI reasons when
the back-end behaviour is correct, undermining the diagnostic value of
the suite.]
```

---

## Pair AP2-5.2-005 — integration test confused with automation harness

```
source_id: xiigen-testing-batch-002-AP2-5.2-005
source_license_state: first_party
pair_kind: dpo_test
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 5.2
rejected_sub_phase: 7.1
```

Prompt: `"design integration tests for the search service against an Elasticsearch container — index creation, document indexing, query semantics, and faceting"`

```
CHOSEN — 5.2 Integration Tests:
Tests run against a real Elasticsearch container. The behaviour under test
is Elasticsearch's actual indexing, query, and aggregation semantics.

Container fixture:
  Testcontainers brings up Elasticsearch 8.x with one node. The fixture is
  shared across the test collection.

Per-test setup:
  Each test class gets its own index name (search_test_<random>) so parallel
  test classes do not collide. The test bootstraps the index with the
  production mapping at the start of the class.

Test catalogue:

Index creation:
  T1 — create the index → mapping shows the expected field types
        (keyword for productId, text for productName with the english
        analyzer, double for price, geo_point for store_location).
  T2 — attempt to create the index when it already exists → the bootstrap
        step is idempotent (does not throw).

Document indexing:
  T3 — index a document → GET /<index>/_doc/<id> returns the document.
  T4 — index a document with a field not in the mapping → the indexing
        succeeds (Elasticsearch dynamic mapping). Assert the new field
        appears in the mapping after indexing.
  T5 — index a document with a field whose value cannot be coerced
        to its mapped type (a string into a double field) → indexing
        returns an error and the document is not indexed.

Query semantics:
  T6 — search for "leather wallet" against a document containing
        "Leather Wallets in Brown" → match (the english analyzer
        handles plural and case).
  T7 — search for "wallt" (typo) → no match unless fuzziness is enabled;
        with fuzziness, match. The chosen mapping does not enable fuzzy
        by default; the test asserts no match.

Faceting:
  T8 — query that returns 100 documents with a category aggregation →
        the response includes the aggregation buckets with the expected
        category counts.

Per-test cleanup:
  Refresh the index between tests so that recently indexed documents are
  searchable in the next test (Elasticsearch's near-real-time indexing
  needs an explicit refresh to be deterministic).

REJECTED — 7.1 Automated Test Harness:
Build a reusable test harness for Elasticsearch tests. Create a base class
ElasticsearchTestBase with a TestContainerFixture, a RunQuery helper, an
AssertSearchResults helper, and a CreateTestIndex helper. Add NUnit and
xUnit adapters so the harness works in either framework. Document the
harness on the team wiki. Add a CI job that runs sample tests against
the harness to verify it works. Other teams can adopt the harness for
their own integration tests.
[WRONG: "design integration tests" asks for the test cases that prove the
search service's correctness against a real Elasticsearch container. Building
a reusable test harness is a different sub-phase (7.1) — the harness is
infrastructure that other tests consume. The prompt asks for the tests
themselves, not the framework that other tests would be written in. Both
sub-phases have value but they are different work products and they are
ordered: the tests for the search service can use the harness when it
exists, but the harness itself is not what verifies the search service.]
```
