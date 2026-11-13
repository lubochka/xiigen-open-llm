# Layer 2 — Nine-Node Flow Pairs (010)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: nine_node_flow | SubPhase: 5.1

---

## N010 — UnitTestingFlow51: EmailDispatcherService (retry logic)

Request: "Spec the EmailDispatcherService retry logic — assert it retries 3 times before failing."

### Node 1 — Request Classification
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1 | TransitionType: feature_addition
  Evidence: "spec" → TEST+IMPLEMENT; "assert" → TEST

### Node 2 — Context Assembly
  Service: EmailDispatcherService | Domain: notifications/messaging
  Dependencies identified: ISmtpClient, IRetryPolicy
  Testable units: dispatch(email), retryOnFailure(email, attempt), isMaxRetriesExceeded(attempt)

### Node 3 — Plan Generation
  Plan:
    3.1 dispatch succeeds on first attempt → expect Ok, MockSmtp called once
    3.2 dispatch fails first attempt, succeeds second → expect Ok, MockSmtp called twice
    3.3 dispatch fails all 3 attempts → expect Fail("MAX_RETRIES_EXCEEDED")
    3.4 isMaxRetriesExceeded(3) → expect true; isMaxRetriesExceeded(2) → expect false
    3.5 dispatch with null email address → expect Fail("INVALID_EMAIL")

### Node 4 — Dependency Resolution
  Mocks: MockSmtpClient (configurable failure count), MockRetryPolicy

### Node 5 — Implementation (Test Code)
  Tests produced: 10 | MockSmtpClient.CallCount asserted in retry tests

### Node 6 — Verification
  All 10 pass | Retry count assertions confirmed via mock call tracking

### Node 7 — Quality Gate
  Status: PASS | Retry boundary (attempt 3 vs attempt 2) explicitly covered

### Node 8 — Output Assembly
  OutputType: TestSuite | Files: EmailDispatcherServiceTests.cs

### Node 9 — Result
  Result: PASS | Tests: 10 | Status: green
