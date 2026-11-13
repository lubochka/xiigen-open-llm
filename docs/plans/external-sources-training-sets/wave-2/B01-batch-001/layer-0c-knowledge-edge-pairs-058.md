# Layer 0-C — Knowledge-Edge Pairs (058)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-058-001 — CommentThread: moderation use cases fire DOCUMENT not KNOWLEDGE_EDGE

```
source_id: xiigen-b01-0c-058-001
source_license_state: first_party
pair_kind: knowledge_edge
```

Boundary case clarification from file 058:
  "Document comment thread moderation use cases" → NOT KNOWLEDGE_EDGE.
  DOCUMENT fires on "document" and "use cases".
  Single predicate: CompiledPredicateIds: ["DOCUMENT"] → routes to 2.1.

Key: "use cases" fires DOCUMENT (confirmed in DOCUMENT lexicon).
"moderation", "flagging", "escalation" do NOT fire any predicate.
The predicate fires via "document" + "use cases" — not via the domain nouns.

---

## B01-0C-058-002 — "notification" ARCHITECTURE context

```
source_id: xiigen-b01-0c-058-002
source_license_state: first_party
pair_kind: knowledge_edge
```

"notification" as ARCHITECTURE term:
  "notification" appears in ARCHITECTURE lexicon under "infrastructure" context.
  "define the notification schema" → ARCHITECTURE fires on "schema" (and "notification" as infrastructure term).
  "implement the notification service" → ARCHITECTURE fires on "service"; IMPLEMENT fires on "implement".
  
Contrast — when "notification" does NOT fire ARCHITECTURE:
  "Show a notification to the user" → "notification" as UI action context → borderline.
  "Design the notification center screen" → UI fires on "screen" (notification center is screen context).
  
Rule: "notification" as infrastructure component (service, schema, pipeline) → ARCHITECTURE.
"notification" as visible UI element (screen, view, center) → UI fires via co-occurring UI terms.
"notification" alone without context → conservative: does not fire.
