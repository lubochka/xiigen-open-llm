# Layer 3 — Teaching Unit Pairs (010)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: teaching-unit

---

## B17-TU-010-001 — v6 corruption patterns in B17 (v6-01, v6-02, v6-03 with examples)

**Three v6 corruption types in B17:**

| Code | Pattern | Trigger | Error type |
|---|---|---|---|
| v6-01 | Predicate invention | "integrate" → IMPLEMENT invented | Compiler error |
| v6-02 | Gap suppression | "token" present → SECURITY not fired | Compiler error |
| v6-03 | Wrong sub-phase | IMPLEMENT + connector → 4.1 instead of 4.3 | Router error |

---

## B17-TU-010-002 — v6-01 deep dive: why "integrate" causes predicate invention

**What happens:** The compiler sees "integrate" and reasons that integration work requires implementation, so it invents IMPLEMENT.

**Why it's wrong:** The classification rules are closed — each vocabulary item maps to exactly one or zero predicates. "integrate" maps to API_CONTRACT. The reasoning "this implies implementation" is semantic drift outside the closed rule set.

**Detection:** Any output where CompiledPredicateIds contains IMPLEMENT but the input had no build verb (build/implement/write/create/code/develop/refactor) is v6-01.

**Prevention:** Train on Rule 1 repeatedly with varied phrasings. The rule is absolute.

---

## B17-TU-010-003 — v6-02 deep dive: why "token" causes gap suppression

**What happens:** The compiler detects "token" in the context of an OAuth flow and collapses it into AUTHORIZATION (reasoning: "it's an OAuth token, so it's authorization"). SECURITY is never fired.

**Why it's wrong:** "token" → SECURITY is unconditional. It doesn't matter what kind of token or what context. The rules don't have conditional exceptions for token type.

**Detection:** Any output where "token" appears in the input but SECURITY is absent from CompiledPredicateIds.

**Prevention:** Train on Rule 6 as an absolute: "token" ALWAYS fires SECURITY. No exceptions.

---

## B17-TU-010-004 — v6-03 deep dive: why "connector" causes wrong sub-phase

**What happens:** The router sees IMPLEMENT and looks up its default sub-phase mapping (4.1 for backend services). It doesn't check for integration vocabulary override.

**Why it's wrong:** The routing rule for IMPLEMENT is two-part: default 4.1, BUT integration vocabulary overrides to 4.3. The router must check for override before applying default.

**Detection:** Any output where IMPLEMENT routes to 4.1 when "connector", "webhook", or "API" was in the input.

**Prevention:** The router's IMPLEMENT handling must have an integration-vocabulary check as a mandatory first step before applying the 4.1 default.
