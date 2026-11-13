# Layer 0-C — Knowledge Edge Pairs (060) — Complete utility-domain gap reference (Round 3 confirmed)
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E060-001 — Complete utility operation verb gap list (Round 3 confirmed)

Utility operation verbs that always produce zero predicates:
  parse, split, tokenise, trim, normalise — string processing verbs
  format, round, truncate, floor, ceil, pad, localise — number/currency format verbs
  convert, transform, scale, translate (measurement) — unit/timezone conversion verbs
  calculate, compute, determine, derive, estimate, measure, aggregate — computation verbs
  match, pattern-match — regex verbs
  produce, output, emit, render, display — output verbs
  configure, set up, initialise, register — setup verbs (NOT in IMPLEMENT lexicon)

---

## E060-002 — "update" vs "extend" boundary: confirmed across all utility domains

"update" → NOT in IMPLEMENT lexicon; always zero predicates.
"extend" → IS in IMPLEMENT lexicon; fires IMPLEMENT.
This is a recurring trap: "update" sounds like a construction verb but is not in the lexicon.
IMPLEMENT fires only on: write, create, build, develop, implement, add, extend, generate.
"update", "modify", "change", "fix", "patch", "adjust", "tweak", "revise", "configure" → all zero predicates.

---

## E060-003 — "should" + domain behaviour does NOT fire predicates

"The [class] should [do something]." → Always zero predicates.
"should" as a modal expressing design intent never fires predicates.
Only explicit action verbs from the predicate lexicons (write/assert/validate etc.) fire predicates.
Modal descriptions of intended behaviour are knowledge-edge gap requests.

---

## E060-004 — Complete utility abbreviation gap list (Round 3 confirmed)

Utility abbreviations that never fire predicates:
  UTC (Coordinated Universal Time), DST (Daylight Saving Time), IANA (Internet Assigned Numbers Authority)
  GPS (Global Positioning System), ISO (International Organisation for Standardisation)
  GBP (Great British Pound), EUR (Euro), USD (US Dollar), JPY (Japanese Yen)
  CSV (Comma-Separated Values), JSON (JavaScript Object Notation), XML (Extensible Markup Language)
  All are domain vocabulary. Predicates fire from action verbs and test-infrastructure vocabulary only.
