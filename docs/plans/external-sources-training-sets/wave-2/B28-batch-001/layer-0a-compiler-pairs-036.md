# Layer 0-A — Compiler Pairs (036) — Complete QA Reference (Round 2)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-036-001 — Complete QA trigger list for Round 2 gate topics

QA fires on (Round 2 gate domain):
  "QA" abbreviation in any position: QA sign-off, QA review, QA gate, QA approve, QA the [noun]
  "ensure quality" → gap (spelled-out form does NOT fire QA)
  "quality check" → gap (no abbreviation, no predicate)
  All Round 2 gate artifacts (SmokeTestSuite, RegressionGate, PerformanceGate, etc.) paired with QA → QA fires on QA abbreviation only
  CompiledPredicateIds: reference

---

## B28-compiler-036-002 — Gate artifact + QA: TEST fires on gate, QA fires on abbreviation

A: "Run the SmokeTestSuite and QA the results."
  "SmokeTestSuite" → TEST | "QA" → QA (abbreviation verb)
  Teaching: Gate artifacts fire TEST; QA fires only on the "QA" abbreviation in the sentence.
  CompiledPredicateIds: ["TEST", "QA"]

---

## B28-compiler-036-003 — Multiple gates + IMPLEMENT + QA

A: "Implement the gate runner service, run the RegressionGate and SecurityScanGate, and QA sign-off."
  "implement" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "RegressionGate" → TEST | "SecurityScanGate" → SECURITY | "QA sign-off" → QA
  Teaching: Four distinct predicates. QA is always last.
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "SECURITY", "QA"]

---

## B28-compiler-036-004 — "ensure quality" vs "QA" contrast

A: "Ensure quality of the release" vs "QA the release."
  "ensure quality" → gap (spelled-out, no QA abbreviation) | "QA the release" → QA (abbreviation)
  Teaching: Only the abbreviation "QA" fires the predicate. Spelled-out quality phrases are gap.
  CompiledPredicateIds for "ensure quality": [] → gap | CompiledPredicateIds for "QA the release": ["QA"]

---

## B28-compiler-036-005 — QA absolute last in Round 2 multi-predicate set

A: "Design the gate architecture, build the gate service, run tests, document criteria, and QA approve."
  Predicate set: ["ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT", "QA"]
  Sequential order: ARCHITECTURE → IMPLEMENT → TEST → DOCUMENT → QA
  Teaching: QA is always absolute last in any multi-predicate sequence. No predicate follows QA.
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT", "QA"]
