# Layer 0-C — Knowledge-Edge Pairs (056)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-056-001 — ColorPicker: "color" IS in UI lexicon — NOT a gap

```
source_id: xiigen-b01-0c-056-001
source_license_state: first_party
pair_kind: knowledge_edge
```

Important distinction from file 056:
  "color" IS in UI lexicon — confirmed from file 017.
  "Build a color picker component" → UI fires on both "color" and "component".
  "Create a color swatch" → UI fires on "color"; "swatch" does NOT fire UI.

Contrast — when "color" does NOT make UI fire alone:
  "Define the color token schema" → "color" IS UI; "define" is gap verb.
  BUT: ARCHITECTURE fires on "schema" independently.
  Result: CompiledPredicateIds: ["ARCHITECTURE"] — not ["UI", "ARCHITECTURE"].
  
Why UI does NOT fire on "color" in "define the color token schema":
  UI fires on UI lexicon terms when the REQUEST is UI-oriented.
  A schema definition request is ARCHITECTURE-oriented; "color" alone is not enough to trigger UI routing when the predicate verb context is absent.
  Rule: UI requires at least one UI action verb OR strong UI action context.
  "Define schema" is ARCHITECTURE context, not UI context — "color" is incidental.

---

## B01-0C-056-002 — "color" context dependency in detail

```
source_id: xiigen-b01-0c-056-002
source_license_state: first_party
pair_kind: knowledge_edge
```

"color" parsing in different request contexts:

  "build a color picker" → UI fires on "color" (UI action context) + IMPLEMENT on "build"
  "design color palette" → UI fires on "color" (UI action context via "design" near UI term)
  "define color token schema" → ARCHITECTURE fires on "schema"; "color" incidental to schema definition
  "review color contrast" → UI fires on "color" ("review" is near UI term in accessibility context)
  "color is not accessible" → UI fires on "color" and on "accessible" (accessibility variant)
  "color space conversion" → "color" alone with no UI/action context → borderline; may not fire

Default rule: When in doubt, if "color" appears with any design/visual context → UI fires.
If "color" appears only in a technical/algorithmic context with no design intent → borderline, conservatively gap.
