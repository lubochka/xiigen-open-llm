# Layer 3 — Teaching Unit Pairs (017)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: teaching-unit | focus: Segment analytics connector

---

## B17-TU-017-001 — "data pipeline" does not supply integration vocabulary

**A:** "Implement the analytics data pipeline." Does this route to 4.3 (integration)?

**B:** No. "data pipeline" is an internal architecture pattern term. It does not appear in the integration vocabulary list (connector, webhook, API, endpoint, external service, third-party, gateway, SDK). Without integration vocabulary, IMPLEMENT routes to 4.1.

**Contrast:**
- "Implement the analytics data pipeline" → IMPLEMENT → 4.1 (no integration vocabulary)
- "Implement the Segment analytics connector" → IMPLEMENT + API_CONTRACT → 4.3 (connector)
- "Implement the Segment analytics API client" → IMPLEMENT + API_CONTRACT → 4.3 (API)

The proper noun "Segment" does not supply integration vocabulary. The structural token ("connector", "API") does.

---

## B17-TU-017-002 — "orchestrate" is not an IMPLEMENT trigger

**A:** "Orchestrate the Segment and HubSpot connectors." Does IMPLEMENT fire?

**B:** No. "Orchestrate" is not in the IMPLEMENT trigger list (build, implement, write, create, code, develop, refactor, restructure, rewrite). Despite implying technical coordination work, "orchestrate" does not fire IMPLEMENT.

"connector" → API_CONTRACT fires (Rule 4).
Result: ["API_CONTRACT"] → 3.3 → documentation.

**v6-01 risk:** A model might reason "orchestration = building orchestration code → IMPLEMENT." Correct: only the explicit trigger words fire IMPLEMENT. "orchestrate" is not one of them.

---

## B17-TU-017-003 — Segment "track" and "identify" events are domain vocabulary (no predicate)

**A:** In "Build the Segment track and identify event connector", do "track" and "identify" fire predicates?

**B:** No. "track" and "identify" are Segment-specific event type names. They are domain vocabulary with no predicate assignment. The predicates fire from:
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT

**Result:** ["IMPLEMENT", "API_CONTRACT"]

Domain-specific event names, method names, and terminology never fire predicates unless they happen to match an explicit trigger word (like "token" or "OAuth").

---

## B17-TU-017-004 — Segment connector with "document" → DOCUMENT fires

**A:** "Implement the Segment connector for the data pipeline, and document the event schema and API contract." What predicates fire?

**B:**
- "Implement" → IMPLEMENT
- "connector" → API_CONTRACT (Rule 4)
- "API" → API_CONTRACT (reinforces)
- "document" → DOCUMENT (Rule 12)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
**SubPhase:** 4.3 + 3.3 + 2.1

**Note:** "event schema" contains "schema" — schema is an API_CONTRACT context reinforcer. DOCUMENT fires from the word "document" explicitly in the request.
