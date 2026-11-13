# Layer 2 — Nine-Node Flow Pairs (006)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: nine_node_flow | SubPhase: 5.1

---

## N006 — UnitTestingFlow51: UserRegistrationService (validation + uniqueness)

Request: "Write unit tests for the UserRegistrationService covering email uniqueness and password strength."

### Node 1 — Request Classification
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1 | TransitionType: feature_addition
  Evidence: "write" → IMPLEMENT; "unit tests" → TEST

### Node 2 — Context Assembly
  Service: UserRegistrationService | Domain: identity/authentication
  Dependencies identified: IUserRepository, IPasswordHasher
  Testable units: register(email, password), isEmailUnique(email), validatePasswordStrength(password)

### Node 3 — Plan Generation
  Plan:
    3.1 register with new email and strong password → expect Ok(userId)
    3.2 register with duplicate email → expect Fail("EMAIL_ALREADY_EXISTS")
    3.3 register with weak password (< 8 chars) → expect Fail("PASSWORD_TOO_WEAK")
    3.4 register with null email → expect Fail("INVALID_EMAIL")
    3.5 validatePasswordStrength("P@ssw0rd!") → expect true
    3.6 validatePasswordStrength("pass") → expect false

### Node 4 — Dependency Resolution
  Mocks: MockUserRepository (returns duplicate on seeded email), MockPasswordHasher

### Node 5 — Implementation (Test Code)
  Tests produced: 13 | xUnit | Theory for password strength variants

### Node 6 — Verification
  All 13 pass

### Node 7 — Quality Gate
  Status: PASS | All validation branches covered

### Node 8 — Output Assembly
  OutputType: TestSuite | Files: UserRegistrationServiceTests.cs

### Node 9 — Result
  Result: PASS | Tests: 13 | Status: green
