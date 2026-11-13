# Layer 3 — Teaching Unit Pairs
# Batch: B32-batch-001
# Scenario: B32 — Vibe coding gap
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: teaching_unit

---

## Pair B32B1-WDA-teach-001 — style vocabulary never suppresses action predicates

A: "Build something quick and dirty for the login." Does IMPLEMENT fire?
B: Yes. "build" fires IMPLEMENT. "quick and dirty" is style vocabulary with no predicate.
Style qualifiers never suppress action predicates — they produce CapabilityGapWarnings only.

---

## Pair B32B1-WDA-teach-002 — "prototype" requires an action verb to route

A: "Vibe a prototype." vs "Build a prototype." Which routes?
B: "Vibe a prototype" → zero predicates → knowledge_edge. "Build a prototype" → IMPLEMENT fires on "build" → routes.
"Prototype" alone is gap vocabulary. It does not fire IMPLEMENT. The action verb is required.

---

## Pair B32B1-WDA-teach-003 — vibe gap DomainId vs project-management gap DomainId

A: What is the DomainId when pure vibe vocabulary fires zero predicates?
B: DomainId: "software-development" (not "project-management"). Vibe coding is software development
with an unrecognized style. The gap is in style vocabulary, not in domain.

---

## Pair B32B1-WDA-teach-004 — "dashboard" is not in UI lexicon

A: "Vibe code a dashboard." Does UI fire?
B: No. "dashboard" is not in UI lexicon. UI fires on: screen, page, interface, component, form,
layout, button, input, widget, modal. "Dashboard" is a product feature term. Zero predicates → knowledge_edge.

---

## Pair B32B1-WDA-teach-005 — quality qualifiers ("good enough", "nothing fancy") are pure gap

A: "Create a good enough solution for the cart — nothing fancy." Which predicates fire?
B: IMPLEMENT fires on "create". "good enough", "nothing fancy" are gap style vocabulary — they
produce CapabilityGapWarnings but do not fire any predicate. CompiledPredicateIds: ["IMPLEMENT"].
Routing proceeds normally via IMPLEMENT despite the vibe framing.
