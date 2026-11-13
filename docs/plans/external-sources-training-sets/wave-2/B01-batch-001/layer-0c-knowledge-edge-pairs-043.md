# Layer 0-C — Knowledge-Edge Pairs (043)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-043-001 — ErrorBoundary: "add" fires IMPLEMENT — not a gap

```
source_id: xiigen-b01-0c-043-001
source_license_state: first_party
pair_kind: knowledge_edge
```

Boundary clarification — "add" as IMPLEMENT verb:
  "Add error boundary wrappers to all production routes" → IMPLEMENT fires on "add".
  "add" IS in IMPLEMENT lexicon: build, implement, create, scaffold, write, add, develop.
  No UI terms → IMPLEMENT only → routes to 4.1.
  This is NOT knowledge-edge.

Compare with gap verbs:
  "set up error boundaries in production" — "set up" NOT in IMPLEMENT lexicon → KNOWLEDGE_EDGE.
  "enable error boundary globally" — "enable" NOT in IMPLEMENT lexicon → KNOWLEDGE_EDGE.
  "wire up error boundaries" — "wire up" NOT in IMPLEMENT lexicon → KNOWLEDGE_EDGE.

Rule: "add" fires IMPLEMENT; "set up", "enable", "wire up", "configure" do NOT.

---

## B01-0C-043-002 — ErrorBoundary: UI fires on "ui" as token even in compound phrases

```
source_id: xiigen-b01-0c-043-002
source_license_state: first_party
pair_kind: knowledge_edge
```

"ui" token in compound phrases:
  "displays a fallback UI" → "ui" fires UI predicate (token match).
  "catch runtime errors and show UI" → "ui" fires UI.
  "display fallback user interface" → "interface" fires UI (UI lexicon includes "interface").
  "catch errors silently" → no UI term → UI does NOT fire.

Rule: Exact token "ui" or "interface" in any position in the request fires UI predicate.
"fallback", "error", "runtime" do NOT fire any predicate.
