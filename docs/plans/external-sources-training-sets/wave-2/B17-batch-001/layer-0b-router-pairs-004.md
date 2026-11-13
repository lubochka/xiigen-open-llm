# Layer 0-B — Router Pairs (004)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: router

---

## B17-0B-004-001 — [API_CONTRACT] only → 3.3 only (NOT 4.3)

**CompiledPredicateIds:** ["API_CONTRACT"]
**Input:** "Integrate the Slack messaging API for sending channel notifications."

**Routing decisions:**

| Predicate | Sub-phase | FlowId |
|---|---|---|
| API_CONTRACT | 3.3 | APIContractFlow33 |

**DecisionCount:** 1
**TransitionType:** documentation

**Critical rule:** API_CONTRACT alone NEVER routes to 4.3. Sub-phase 4.3 requires IMPLEMENT. This is Rule 10: API_CONTRACT alone → 3.3.

---

## B17-0B-004-002 — Nearest wrong for [API_CONTRACT] only routing

**Correct:** API_CONTRACT → 3.3
**Nearest wrong #1:** API_CONTRACT → 4.3 (very common v6-03 variant — integration vocabulary in the input misleads the router to select 4.3)
**Nearest wrong #2:** API_CONTRACT → 4.1 (wrong sub-phase entirely)

**Why models make this error:** The input "integrate the Slack API" contains strong integration vocabulary (integrate, API, Slack). The router may conflate integration vocabulary with 4.3 routing. But 4.3 requires IMPLEMENT, not just integration vocabulary.

**Gate B:** ["API_CONTRACT"] alone → always 3.3. Never 4.3 without IMPLEMENT.

---

## B17-0B-004-003 — Comparison: integrate alone vs build + integrate

**Scenario A:** "Integrate the Slack API." → ["API_CONTRACT"] → 3.3 → documentation
**Scenario B:** "Build and integrate the Slack API connector." → ["IMPLEMENT", "API_CONTRACT"] → 4.3 + 3.3 → feature_addition

**DecisionCount comparison:**
- Scenario A: 1 decision (API_CONTRACT → 3.3)
- Scenario B: 2 decisions (IMPLEMENT → 4.3, API_CONTRACT → 3.3)

**The routing change is dramatic:** adding "build" changes the sub-phase from 3.3 to 4.3 + 3.3, changes DecisionCount from 1 to 2, and changes TransitionType from documentation to feature_addition.

---

## B17-0B-004-004 — [API_CONTRACT] routing with documentation TransitionType

**TransitionType when API_CONTRACT alone:**
Rule 17: API_CONTRACT alone (no IMPLEMENT) → TransitionType: documentation

**This holds regardless of input phrasing:**
- "Integrate the API" → documentation
- "Wire up the webhook endpoint" (no build verb) → documentation
- "Connect the external service" (no build verb) → documentation

**All of the above produce:** CompiledPredicateIds: ["API_CONTRACT"], SubPhase: 3.3, TransitionType: documentation, DecisionCount: 1
