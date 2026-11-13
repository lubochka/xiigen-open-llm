# Held-Out and Corruption Pairs (007)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption

---

## B17-HO-007-001 — HELD-OUT: "write integration tests" + "build connector" → IMPLEMENT + API_CONTRACT + TEST

**Input:**
"Build the Twilio SMS connector and write integration tests for the send and receive operations."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST"]
SubPhase: 4.3 (IMPLEMENT), 3.3 (API_CONTRACT), 5.1 (TEST)
TransitionType: feature_addition

**Gate B:** TEST must be in output; TEST routes to 5.1 (not 4.3, not 5.3).

---

## B17-HO-007-002 — HELD-OUT: "add tests" phrasing → TEST fires

**Input:**
"Build the Plaid connector and add unit tests for the account linking flow."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST"]
SubPhase: 4.3, 3.3, 5.1
TransitionType: feature_addition

**Key test:** "add tests" → TEST (Rule 13). Same as "write tests" and "create tests".

---

## B17-HO-007-003 — CORRUPTION: TEST routes to 5.3 instead of 5.1

**Input:**
"Build the GitHub webhook connector and write integration tests for the event handlers."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST"]
SubPhase: 4.3, 3.3, 5.3 (WRONG sub-phase for TEST)

**CORRECT output:**
SubPhase: 4.3, 3.3, 5.1

**Error type:** v6-03 (wrong sub-phase). TEST in B17 connector context → 5.1 (Unit and Integration Testing). 5.3 is not the correct destination in B17.

---

## B17-HO-007-004 — CORRUPTION: "integration tests" causes TEST to not fire (only API_CONTRACT fires)

**Input:**
"Build the SendGrid connector and write integration tests for the email pipeline."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
(Error: "integration" in "write integration tests" fired API_CONTRACT, but "write tests" was not recognized as a TEST trigger — so TEST was suppressed)

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST"]

**Error type:** v6-02 — "write integration tests" contains both "integration" (→ API_CONTRACT context) AND "write tests" (→ TEST). Both must fire. The "integration" in "integration tests" doesn't consume the "write tests" trigger.

**Rule:** Vocabulary triggers are cumulative, not exclusive. "write integration tests" activates both API_CONTRACT-context vocabulary AND TEST trigger simultaneously.
