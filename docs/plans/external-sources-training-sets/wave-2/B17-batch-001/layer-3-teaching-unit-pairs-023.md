# Layer 3 — Teaching Unit Pairs (023)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: teaching-unit | focus: Multi-connector orchestration

---

## B17-TU-023-001 — Multi-connector context does not change predicate rules

**A:** If five connectors are named in a single request with "integrate", does IMPLEMENT eventually fire due to the scale of the integration work?

**B:** No. Rule 1 is unconditional: "integrate" fires API_CONTRACT ONLY, regardless of how many connectors are named. There is no "scale exception" to Rule 1. Five connectors + "integrate" → ["API_CONTRACT"] → 3.3 → documentation.

**Teaching point:** Predicate rules are lexical and unconditional. The number of connectors, the complexity of the integration, or the business importance of the work does not change predicate firing. Only the explicit trigger words fire predicates.

---

## B17-TU-023-002 — "bridge" and "orchestrate" are not IMPLEMENT triggers

**A:** "Bridge the HubSpot and Segment connectors." Does IMPLEMENT fire?

**B:** No. "Bridge" is not in the IMPLEMENT trigger list. The IMPLEMENT trigger list contains: build, implement, write, create, code, develop, refactor, restructure, rewrite.

"connector" → API_CONTRACT (fires once for both connectors)
"Bridge" → no predicate

**Result:** ["API_CONTRACT"] → 3.3 → documentation

**v6-01 risk:** "Bridging connectors" sounds like building integration code. Correct: only explicit trigger words fire IMPLEMENT. "Bridge" implies connecting existing components — it maps to API_CONTRACT conceptually, but it fires no predicate. Only "connector" fires.

---

## B17-TU-023-003 — Predicates are sets: multi-connector requests do not multiply predicates

**A:** "Build the Stripe connector, the Avalara connector, and the Zuora connector." How many times does API_CONTRACT fire?

**B:** Once. CompiledPredicateIds is a deduplicated set. "connector" appears three times conceptually, but API_CONTRACT fires once. The output is ["IMPLEMENT", "API_CONTRACT"] — not ["IMPLEMENT", "API_CONTRACT", "API_CONTRACT", "API_CONTRACT"].

**Teaching point:** Predicate sets are deduplicated. This applies to all predicates: IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION, TEST, DOCUMENT, QA. No predicate appears twice in CompiledPredicateIds, regardless of how many trigger occurrences appear in the input.

---

## B17-TU-023-004 — Multi-connector full pipeline teaching summary

**A:** In a multi-connector request with all seven predicates, what changes compared to a single-connector full pipeline?

**B:** The predicate set, routing, factory selection, orchestration plan, and QA position are identical to a single-connector full pipeline. The nine-node flow (IntegrationConnectorFlow43) handles multiple connectors within a single flow execution — it processes each connector as a distinct branch. No additional predicates, sub-phases, or orchestration steps are added by having multiple connectors.

**What does change:** The nine-node flow generates more files (one per connector) and the N08 arbiter reviews completeness for each connector. The orchestration step count remains the same.
