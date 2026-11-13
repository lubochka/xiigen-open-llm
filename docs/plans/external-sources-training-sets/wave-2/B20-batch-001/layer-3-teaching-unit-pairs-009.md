# Layer 3 — Teaching Unit Pairs (009)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU009-001 — Teach: "test-driven" fires TEST weakly; "develop test-driven" fires TEST+IMPLEMENT

Misconception: "test-driven" in a request description fires nothing because it's an adjectival modifier.
Correction: "test" in "test-driven" fires TEST via its component word. The compound modifier does not block the "test" root.
  "Develop the service test-driven." → ["IMPLEMENT", "TEST"]
  "A test-driven approach is preferred." → ["TEST"] (passive preference; no action verb for IMPLEMENT)
Rule: "test-driven" fires TEST via the "test" component. The hyphenated compound form does not suppress the predicate.

---

## TU009-002 — Teach: "spy" and "fake" alone are gap; TEST fires only from "test" presence

Misconception: "use a spy" should fire TEST because spies are test infrastructure.
Correction: "spy" alone is gap. "fake" alone is gap. TEST fires only when "test" appears.
  "Use a spy." → [] (gap — "use" is gap; "spy" is gap)
  "Build a fake for testing." → ["IMPLEMENT", "TEST"] (IMPLEMENT via "build"; TEST via "testing")
  "Add a test spy." → ["IMPLEMENT", "TEST"] (IMPLEMENT via "add"; TEST via "test spy" — "test" present)
Rule: Testing doubles ("spy", "fake", "mock", "stub") are gap alone. Pair with "test" to fire TEST.

---

## TU009-003 — Teach: "performance test" is still TEST; routes to 5.1 not perf sub-phase

Misconception: "performance test" should route to a performance sub-phase, not 5.1.
Correction: "performance test" fires TEST via "test". Routing is 5.1 unless E2E/browser vocabulary appears.
  "Test the performance of the SearchService under load." → ["TEST"] → 5.1
  There is no separate performance sub-phase in the nine-node router. Performance testing ≠ load testing sub-phase.
Rule: Performance threshold vocabulary ("200ms", "1000 RPS", "99th percentile") is gap. TEST fires on "test". Route → 5.1.

---

## TU009-004 — Teach: "verify the fix" fires TEST; "fix" alone is gap

Misconception: "verify the fix" fires TEST (via "verify") and IMPLEMENT (via "fix").
Correction: "verify" fires TEST. "fix" does NOT fire IMPLEMENT. Only TEST fires.
  "Verify the fix." → ["TEST"]
  "Implement the fix and verify it." → ["IMPLEMENT", "TEST"] (IMPLEMENT via "implement"; TEST via "verify")
Rule: "fix" → gap for IMPLEMENT regardless of context. This is a hard boundary, not a soft rule.

---

## TU009-005 — Teach: "increase test coverage" fires IMPLEMENT (via "increase") + TEST (via "test coverage")

Misconception: "increase test coverage" should fire only TEST because it's about testing metrics.
Correction: "increase" fires IMPLEMENT (increasing something = building/adding). "test coverage" fires TEST via "test".
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  "90% coverage" threshold is gap (doesn't fire any predicate alone).
Rule: "increase" is an IMPLEMENT trigger. "coverage" alone is gap; "test coverage" fires TEST via "test".
