# Layer 0-C — Knowledge-Edge Pairs (023)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Multi-connector orchestration edges

---

## B17-0C-023-001 — Edge: predicates are sets — multiple connector mentions do not multiply predicates

**The set rule:**
Predicates are collected into a set (not a multiset). Even if a trigger token appears ten times in the input, the predicate appears once in the output.

**Multi-connector evidence:**
| Input | "connector" occurrences | API_CONTRACT in output |
|---|---|---|
| "Build the Stripe connector" | 1 | Once |
| "Build the Stripe and Avalara connector pair" | 1 | Once |
| "Build the Stripe connector, the Avalara connector, and the Zuora connector" | 3 | Once |

**CompiledPredicateIds is always a deduplicated ordered list.**

---

## B17-0C-023-002 — Edge: "orchestrate" and "coordinate" are not IMPLEMENT triggers

**Input:** "Orchestrate the Stripe, Square, and Avalara connectors for the payment and tax pipeline."

**Edge assertions:**
- "Orchestrate" — NOT in IMPLEMENT trigger list (build, implement, write, create, code, develop, refactor, restructure, rewrite)
- "connector" → API_CONTRACT (Rule 4)

**Result:** ["API_CONTRACT"] — no IMPLEMENT

**Sub-phase:** 3.3 (API_CONTRACT alone → 3.3)
**TransitionType:** documentation

**Teaching point:** "Orchestrate" may imply technical coordination work but it does not fire IMPLEMENT in this corpus. Only the explicit trigger verbs fire IMPLEMENT.

---

## B17-0C-023-003 — Edge: "bridge" and "sync" are not IMPLEMENT triggers

**Input:** "Bridge the HubSpot CRM and Segment analytics connectors to sync contact data."

**Edge assertions:**
- "Bridge" — not an IMPLEMENT trigger
- "connector" → API_CONTRACT (fires once — two connector mentions)
- "sync" — not an IMPLEMENT trigger

**Result:** ["API_CONTRACT"] — no IMPLEMENT

**Routing:** API_CONTRACT → 3.3 (documentation)

**v6-01 risk:** A model might reason "bridging" and "syncing" imply code → IMPLEMENT. Correct: only the explicit trigger words (build, implement, write, create, develop, code, refactor) fire IMPLEMENT.

---

## B17-0C-023-004 — Edge: Rule 1 applies unconditionally in multi-connector context

**Input:** "Integrate the HubSpot, Segment, and Intercom connectors with the central CRM orchestration layer."

**Edge assertions:**
- "Integrate" → API_CONTRACT (Rule 1: integrate fires API_CONTRACT ONLY)
- "connector" → API_CONTRACT (reinforces; fires once — three occurrences)
- No build verb → IMPLEMENT does NOT fire

**Result:** ["API_CONTRACT"]
**DecisionCount:** 1
**SubPhase:** 3.3
**TransitionType:** documentation

**Rule 1 is unconditional and context-independent.** "Integrate" fires API_CONTRACT only, even when three connectors are named, even in a multi-system context, even with CRM and orchestration vocabulary.
