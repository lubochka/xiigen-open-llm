# Layer 3 — Teaching Unit Pairs (002)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: teaching-unit

---

## B17-TU-002-001 — "integrate" alone never fires IMPLEMENT (complete rule with examples)

**The top teaching point for B17.**

**Rule 1 (absolute):** "integrate" fires API_CONTRACT ONLY. It never fires IMPLEMENT.

**Why this matters:** "integrate" is the most common source of v6-01 predicate invention errors. Models may reason: "integrate means to build something that connects systems — that sounds like IMPLEMENT." This reasoning is WRONG for B17.

**Complete rule:**
- "integrate" → API_CONTRACT (always)
- "integrate" → NOT IMPLEMENT (never)

---

## B17-TU-002-002 — Positive examples: "integrate" alone → ["API_CONTRACT"]

| Input | Predicates | Sub-phase |
|---|---|---|
| "Integrate the Stripe API" | ["API_CONTRACT"] | 3.3 |
| "Integrate the Slack messaging API" | ["API_CONTRACT"] | 3.3 |
| "Integrate the PayPal gateway" | ["API_CONTRACT"] | 3.3 |
| "Integrate the Twilio SMS service" | ["API_CONTRACT"] | 3.3 |
| "Integrate the external payment system" | ["API_CONTRACT"] | 3.3 |

**All five routes to 3.3, not 4.3.** TransitionType: documentation for all five.

---

## B17-TU-002-003 — Contrast: "integrate" + build verb → adds IMPLEMENT

| Input | Predicates | Sub-phase |
|---|---|---|
| "Build and integrate the Stripe connector" | ["IMPLEMENT", "API_CONTRACT"] | 4.3, 3.3 |
| "Implement and integrate the Slack API" | ["IMPLEMENT", "API_CONTRACT"] | 4.3, 3.3 |
| "Create the integration for PayPal" | ["IMPLEMENT", "API_CONTRACT"] | 4.3, 3.3 |

**The build verb is the IMPLEMENT trigger. "integrate" alone is not sufficient.**

---

## B17-TU-002-004 — The exact failure mode: v6-01 from "integrate"

**Wrong reasoning (v6-01):**
"integrate → building an integration → IMPLEMENT"

**Correct reasoning:**
"integrate → connecting to an API → API_CONTRACT"

**Mnemonic:** "integrate" describes the goal (connect systems). IMPLEMENT describes the method (write code). The word "integrate" describes what you want to achieve, not that you're going to write code to achieve it. You might integrate an API by writing code (IMPLEMENT) or by configuring an existing component (no IMPLEMENT). The word "integrate" alone doesn't commit to either — it commits only to API_CONTRACT (the connection being established).
