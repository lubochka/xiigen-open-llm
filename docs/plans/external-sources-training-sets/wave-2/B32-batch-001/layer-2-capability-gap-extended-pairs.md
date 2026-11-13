# Layer 2 — Capability Gap Extended Pairs (replaces nine-node layer)
# Batch: B32-batch-001
# Scenario: B32 — Vibe coding gap
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: capability_gap_extended

---

## Pair B32B1-WDA-gap-001 — vibe coding vocabulary map

```
source_id: xiigen-b32-gap-001
pair_kind: capability_gap_extended
```

A: Identify all vibe coding gap terms and their gap IDs.

B:
  GapVocabularyMap:
    - term: "vibe" / "vibe code" / "vibing"
      predicate_match: null
      gap_id: "gap-vibe-coding-style-001"
      notes: "Style descriptor for intuition-driven, exploratory coding; no predicate"

    - term: "prototype" (standalone)
      predicate_match: null
      gap_id: "gap-vibe-coding-prototype-001"
      notes: "Requires IMPLEMENT co-occurrence ('create prototype') to route; alone = gap"

    - term: "exploratory"
      predicate_match: null
      gap_id: "gap-vibe-coding-exploratory-001"
      notes: "Development mode descriptor; no predicate"

    - term: "quick and dirty"
      predicate_match: null
      gap_id: "gap-vibe-coding-style-001"
      notes: "Quality/style qualifier; does not affect predicate compilation"

    - term: "good enough"
      predicate_match: null
      gap_id: "gap-vibe-coding-style-001"
      notes: "Acceptance criterion style; no predicate"

    - term: "throw something together"
      predicate_match: null
      gap_id: "gap-vibe-coding-style-001"
      notes: "Informal development descriptor; no predicate"

    - term: "feels right" / "intuitive feel"
      predicate_match: null
      gap_id: "gap-vibe-coding-ux-feel-001"
      notes: "Subjective UX descriptor; no predicate"

    - term: "just get it working"
      predicate_match: null
      gap_id: "gap-vibe-coding-style-001"
      notes: "Minimal-viable framing; no predicate"

  DomainId_when_zero_predicates: "software-development"
  Note: "Unlike project-management gap batches (B36, B37), vibe coding gaps belong to
    the software-development domain — the activity is code-writing, the gap is in
    style vocabulary."

---

## Pair B32B1-WDA-gap-002 — "dashboard" is NOT in UI lexicon

```
source_id: xiigen-b32-gap-002
pair_kind: capability_gap_extended
```

A: "Vibe code a dashboard for the admin panel." Does UI fire on "dashboard"?

B: No. "dashboard" is NOT in the UI predicate lexicon. UI fires on: screen, page, interface,
component, form, layout, button, input, view, widget, modal, dropdown, navigation, menu,
responsive. "Dashboard" is a product/feature term, not a UI element term. "Admin panel"
also does not fire UI. Result: zero predicates → knowledge_edge.
Gap ID: gap-vibe-coding-exploratory-001 (vibe vocabulary) + gap-ui-dashboard-001 (dashboard gap).

---

## Pair B32B1-WDA-gap-003 — vibe with TEST boundary

```
source_id: xiigen-b32-gap-003
pair_kind: capability_gap_extended
```

A: "Let's exploratorily test the checkout flow — vibe through the user journey."

B:
  CompiledPredicateIds: ["TEST"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-vibe-coding-exploratory-001: 'exploratorily', 'vibe through' are gap vocabulary"

Predicate analysis: "test" → TEST fires on "test". "checkout flow" — no predicate.
"vibe through" — gap. "user journey" — not in UI predicate directly.
Result: ["TEST"]. Teaching: TEST fires on "test" even when the overall framing is
exploratory/vibe. Quality style vocabulary does not suppress predicate matches.

---

## Pair B32B1-WDA-gap-004 — DomainId for vibe coding gap: "software-development"

```
source_id: xiigen-b32-gap-004
pair_kind: capability_gap_extended
```

A: "When pure vibe vocabulary fires zero predicates, what DomainId does the orchestrator emit?
Why is it different from B36/B37 which emit 'project-management'?"

B:
  DomainId: "software-development"
  Reason: "Vibe coding is a style of software development. The gap is in style/mode vocabulary,
    not in domain vocabulary. The activity (writing code, building features) is clearly in the
    software-development domain. Contrast with B36 (sprint, roadmap = project planning domain)
    and B37 (track, coordinate = project managing domain) where the domain itself is different.
    Vibe coding requests are about the same domain (software-development) — the orchestrator
    just can't route them because the style vocabulary has no predicate."
