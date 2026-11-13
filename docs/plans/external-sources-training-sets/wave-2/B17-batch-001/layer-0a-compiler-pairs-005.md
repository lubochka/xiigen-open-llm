# Layer 0-A — Compiler Pairs (005)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: compiler

---

## B17-0A-005-001 — "integrate" alone fires API_CONTRACT only (NOT IMPLEMENT)

**Input:**
"Integrate the Slack messaging API for sending channel notifications."

**Compiler analysis:**
- "Integrate" → API_CONTRACT (Rule 1: "integrate" fires API_CONTRACT ONLY)
- "API" → API_CONTRACT (reinforces)
- NO build/implement/write/create/code/develop verb → IMPLEMENT does NOT fire
- CRITICAL RULE: "integrate" never fires IMPLEMENT

**CompiledPredicateIds:** ["API_CONTRACT"]
**SubPhase:** 3.3 (API_CONTRACT alone → 3.3, NOT 4.3 — Rule 10)
**TransitionType:** documentation (Rule 17: API_CONTRACT alone → documentation)
**DomainId:** software-development

**Teaching point (top):** "integrate" is the most common v6-01 error source. "integrate" → API_CONTRACT ONLY. Never IMPLEMENT.

---

## B17-0A-005-002 — "integrate" + "build" → both IMPLEMENT and API_CONTRACT

**Input:**
"Build and integrate the Slack messaging API for channel notifications."

**Compiler analysis:**
- "Build" → IMPLEMENT (Rule 11)
- "integrate" → API_CONTRACT (Rule 1)
- "API" → API_CONTRACT (reinforces)
- Integration vocabulary present → IMPLEMENT routes to 4.3

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**SubPhase:** 4.3
**TransitionType:** feature_addition
**DomainId:** software-development

**Teaching point:** Adding "build" alongside "integrate" fires IMPLEMENT. The predicate set changes from ["API_CONTRACT"] to ["IMPLEMENT", "API_CONTRACT"] and routing changes from 3.3 to 4.3.

---

## B17-0A-005-003 — "integrate" + "implement" → IMPLEMENT + API_CONTRACT

**Input:**
"Implement and integrate the Slack notification connector."

**Compiler analysis:**
- "Implement" → IMPLEMENT (Rule 11)
- "integrate" → API_CONTRACT (Rule 1)
- "connector" → API_CONTRACT (Rule 4; reinforces)
- Integration vocabulary → 4.3

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**SubPhase:** 4.3
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-005-004 — "integrate" alone: near-miss with "connect" comparison

**Input (A — integrate only):**
"Integrate the Slack API for team notifications."
→ API_CONTRACT only → 3.3 → documentation

**Input (B — connect only):**
"Connect the Slack API to the notification pipeline."
→ API_CONTRACT only → 3.3 → documentation

**Compiler analysis (both):**
Both "integrate" and "connect" fire API_CONTRACT without a build verb. Both route to 3.3. TransitionType: documentation for both. The key difference is only visible when a build verb is added.

**Teaching point:** "integrate", "connect", and "wire up" are API_CONTRACT vocabulary. None fire IMPLEMENT. All require a separate build verb to add IMPLEMENT.
