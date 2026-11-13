# Held-Out and Corruption Pairs (018)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-018 — Held-out: "spec" fires DOCUMENT not API_CONTRACT for reporting API

```
source_id: xiigen-b12-ho-018
pair_kind: held_out
hold_out_reason: "Tests that 'spec' fires DOCUMENT only, even when appearing alongside API terms"
```

A:
  user_request: "Write the API spec for the financial reporting endpoints."

B (expected):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Routing: ["3.3", "2.1"]
  TransitionType: "feature_addition"
  Reason: "api" + "endpoints" → API_CONTRACT; "spec" → DOCUMENT (spec is a documentation term); "write" → IMPLEMENT fires
  Full set: ["API_CONTRACT", "IMPLEMENT", "DOCUMENT"]

Held-out evaluation:
  Pass: CompiledPredicateIds contains "API_CONTRACT" and "DOCUMENT"; "spec" does NOT fire API_CONTRACT
  Fail (v6-01): "spec" wrongly fires API_CONTRACT in addition to DOCUMENT

---

## B12-COR-018 — Corruption: "spec" wrongly fires API_CONTRACT

```
source_id: xiigen-b12-cor-018
corruption_type: v6-01
pair_kind: corruption
```

A: same request as B12-HO-018

B (corrupt):
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "DOCUMENT"]
  CorruptReason: "v6-01: classifier fired API_CONTRACT on 'spec' because 'spec' appeared near 'api'; 'spec' fires DOCUMENT only, never API_CONTRACT"

Note: The predicate set looks correct here because "api" + "endpoints" already fire API_CONTRACT independently.
The corruption is in the reasoning — API_CONTRACT should be attributed to "api"/"endpoints", NOT to "spec".

Corrupt reasoning: API_CONTRACT fired because of "spec"
Correct reasoning: API_CONTRACT fired because of "api" + "endpoints"; "spec" → DOCUMENT only

---

## B12-COR-018B — Corruption: IMPLEMENT suppressed on "write the API spec"

```
source_id: xiigen-b12-cor-018b
corruption_type: v6-01
pair_kind: corruption
```

A: same request as B12-HO-018

B (corrupt):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]  ← IMPLEMENT missing
  CorruptReason: "v6-01: classifier saw 'write' as a composition verb (write documentation) and did not fire IMPLEMENT; 'write' is a first-class IMPLEMENT verb"

Correct: ["API_CONTRACT", "IMPLEMENT", "DOCUMENT"]
Corrupt: ["API_CONTRACT", "DOCUMENT"]
Rule: "write" → IMPLEMENT unconditionally; context (writing a spec, writing code, writing docs) does not suppress IMPLEMENT.
