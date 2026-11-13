# Held-Out and Corruption Pairs (015)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-015 — Held-out: "user management API interface" fires both API_CONTRACT and ARCHITECTURE

```
source_id: xiigen-b12-ho-015
pair_kind: held_out
hold_out_reason: "Tests interface disambiguation: API context fires API_CONTRACT; design context fires ARCHITECTURE"
```

A:
  user_request: "Define the user management API interface for authentication and profile operations."

B (expected):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Routing: ["3.3", "2.1"]
  TransitionType: "documentation"
  Reason: "api" + "interface" → API_CONTRACT (interface in API context); "define" → DOCUMENT; "authentication" → SECURITY also fires
  Full set: ["API_CONTRACT", "SECURITY", "DOCUMENT"]

Held-out evaluation:
  Pass: CompiledPredicateIds contains "API_CONTRACT"
  Fail (v6-01): "interface" treated as ARCHITECTURE trigger only → ARCHITECTURE fires instead of API_CONTRACT

---

## B12-COR-015 — Corruption: ARCHITECTURE fires instead of API_CONTRACT on "API interface"

```
source_id: xiigen-b12-cor-015
corruption_type: v6-01
pair_kind: corruption
```

A: same request as B12-HO-015

B (corrupt):
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY", "DOCUMENT"]  ← API_CONTRACT missing, ARCHITECTURE wrong
  CorruptReason: "v6-01: classifier saw 'interface' and fired ARCHITECTURE without recognising 'api' co-occurrence; in API context 'interface' fires API_CONTRACT not ARCHITECTURE"

Correct: ["API_CONTRACT", "SECURITY", "DOCUMENT"]
Corrupt: ["ARCHITECTURE", "SECURITY", "DOCUMENT"]

---

## B12-COR-015B — Corruption: SECURITY suppressed on "authentication" in API definition

```
source_id: xiigen-b12-cor-015b
corruption_type: v6-02
pair_kind: corruption
```

A: same request as B12-HO-015

B (corrupt):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]  ← SECURITY missing
  CorruptReason: "v6-02: classifier saw 'authentication' as part of API scope description and did not fire SECURITY; authentication always fires SECURITY unconditionally (Core Rule 6)"

Correct: ["API_CONTRACT", "SECURITY", "DOCUMENT"]
Corrupt: ["API_CONTRACT", "DOCUMENT"]
Rule: "authentication" → SECURITY always, regardless of sentence context or surrounding API terms.
