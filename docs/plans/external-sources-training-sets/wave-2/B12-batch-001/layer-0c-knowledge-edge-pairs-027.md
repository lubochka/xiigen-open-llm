# Layer 0-C — Knowledge Edge Pairs (027)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-027-001 — WebSocket: "integrate" fires API_CONTRACT only with API co-occurrence

```
source_id: xiigen-b12-0c-027-001
pair_kind: knowledge_edge
```

A:
  CompiledText: "Integrate the WebSocket endpoint for the chat service with the API gateway."

B:
  CompiledPredicateIds: ["API_CONTRACT"]
  Rule applications:
    Rule B12-R17: "integrate" → API_CONTRACT ONLY because "endpoint" and "api" are co-occurring ✓
    Rule B12-R01: "websocket" → API_CONTRACT ✓
    Rule B12-R08: "endpoint" → API_CONTRACT ✓
  Key edge: "integrate" alone (without API context) does NOT fire API_CONTRACT.
  With "endpoint" + "api" present: API_CONTRACT fires.
  Boundary: "Integrate the database records" → NO API_CONTRACT ("integrate" + no API co-occurrence)
            "Integrate the WebSocket endpoint" → API_CONTRACT fires ("integrate" + API co-occurrence)

---

## B12-0C-027-002 — WebSocket schema: fires API_CONTRACT when "websocket" is co-occurrence

```
source_id: xiigen-b12-0c-027-002
pair_kind: knowledge_edge
```

A:
  CompiledText: "Document the WebSocket message schema and connection lifecycle contract for the trading platform."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Rule applications:
    Rule B12-R01: "websocket" → API_CONTRACT ✓
    Rule B12-R02: "schema" fires API_CONTRACT because "websocket" is API co-occurrence ✓
    Rule B12-R01: "contract" → API_CONTRACT unconditional ✓
    Rule B12-R04: "document" → DOCUMENT ✓
  Edge: "connection lifecycle contract" — "contract" fires regardless of modifier ("lifecycle" is the modifier).
  Key: All of these modifiers still fire API_CONTRACT via "contract": lifecycle contract, message contract,
  event contract, protocol contract, payload contract.

---

## B12-0C-027-003 — WebSocket build: IMPLEMENT fires from "build"; both predicates present

```
source_id: xiigen-b12-0c-027-003
pair_kind: knowledge_edge
```

A:
  CompiledText: "Build the WebSocket connection handler and define the message protocol contract."

B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  Rule applications:
    Rule B12-R08: "build" → IMPLEMENT ✓
    Rule B12-R01: "websocket" + "contract" → API_CONTRACT ✓
    Rule B12-R04: "define" → DOCUMENT ✓
    Rule B12-R10: IMPLEMENT → TransitionType = feature_addition ✓
  Edge: "build" is an unconditional IMPLEMENT verb. Even when the rest of the request is about
  defining a contract, "build" fires IMPLEMENT. The clause "and define the message protocol contract"
  does NOT suppress the IMPLEMENT signal from "build".
