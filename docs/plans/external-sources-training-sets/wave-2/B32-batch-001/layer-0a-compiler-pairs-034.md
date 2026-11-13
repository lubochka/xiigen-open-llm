# Layer 0-A — Compiler Pairs (034)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-034-001 — gap + DOCUMENT embedded: DOCUMENT fires alongside gap

**Input:** "Rough draft — document the API endpoints."

```
Token scan:
"rough draft" → gap-vibe-coding-rough-cut-001 (rough draft = rough-cut family)
"document" → DOCUMENT fires ✓
"API endpoints" → scope

Predicates: ["DOCUMENT"]
GapWarnings: [gap-vibe-coding-rough-cut-001]
RouteTarget: DocumentFlow
CapabilityGapWarning: style_qualifier_noted
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
```

---

## B32-0A-034-002 — "good enough" + DOCUMENT

**Input:** "Good enough docs — document the auth flow for the team."

```
Token scan:
"good enough" → gap-vibe-coding-style-001
"document" → DOCUMENT fires ✓
"auth flow" → domain scope

Predicates: ["DOCUMENT"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: DocumentFlow
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0A-034-003 — "minimum viable" + DOCUMENT

**Input:** "Minimum viable docs — just document the critical endpoints."

```
Token scan:
"minimum viable" → gap-vibe-coding-style-001
"document" → DOCUMENT fires ✓
"critical endpoints" → scope

Predicates: ["DOCUMENT"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: DocumentFlow
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0A-034-004 — "hack together" + DOCUMENT

**Input:** "Hack together some docs for the integration."

```
Token scan:
"hack together" → gap-vibe-coding-hack-001
"some docs" → shorthand for documentation; "for the integration" = scope
Note: "hack together" does NOT fire IMPLEMENT (together form → gap only, per Rule established in file 016)
"docs" as noun target — DOCUMENT fires ✓ when "document/docs" is the object

Predicates: ["DOCUMENT"]
GapWarnings: [gap-vibe-coding-hack-001]
RouteTarget: DocumentFlow
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0A-034-005 — "thin slice" + DOCUMENT + IMPLEMENT

**Input:** "Thin slice: implement the feature and document the API."

```
Token scan:
"thin slice" → gap-vibe-coding-spike-001
"implement" → IMPLEMENT fires ✓
"document" → DOCUMENT fires ✓

Predicates: ["IMPLEMENT", "DOCUMENT"]
GapWarnings: [gap-vibe-coding-spike-001]
CapabilityGapWarning: style_qualifier_noted
```

---
