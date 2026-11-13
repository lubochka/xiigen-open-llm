# Layer 0-C — Knowledge-Edge Pairs (012)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B01-0C-012-001 — B01 knowledge edge summary: all gap patterns

```
source_id: xiigen-b01-0c-012-001
source_title: XIIGen B01 knowledge-edge-012 — B01 gap patterns and knowledge edge summary
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

A:
  DocumentId: "B01-0c-012-001-prompt"
  CompiledText: "Summarize all B01 knowledge-edge patterns."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no predicate matched"
  KnownFailureMode: "no_predicate_match"

B:
  FlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  Metrics:
    knowledge_edge_reason: "no_predicate_match"
    matched_predicate_count: "0"
    capability_gap_id: "gap-b01-summary"
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  Warnings: ["knowledge_edge: no predicate matched"]

B01 knowledge-edge patterns (requests that produce empty predicate lists):

Category 1 — Pure navigation intent without UI lexicon terms:
  "Map the navigation structure of the app"
  "Fix the navigation menu"
  "Update the navigation links"
  → All produce [] because "navigation" is not in any lexicon

Category 2 — UX quality language without UI terms:
  "Make the onboarding feel more modern and engaging"
  "Improve the user experience"
  "Redesign the onboarding flow"
  → "modern", "engaging", "improve", "redesign", "feel" — none in any lexicon

Category 3 — Business context without predicate terms:
  "Define our product vision for the onboarding experience"
  "Align stakeholders on onboarding goals"
  → "vision", "align", "goals" — none in any predicate lexicon
  Note: "experience" alone does NOT fire UI (only "ux" does)

Category 4 — Incomplete technical requests:
  "The onboarding screens need work"
  → "screen" fires UI! This is NOT knowledge-edge. "screens" fires UI.
  → This would produce ["UI"] → routes to 1.1.
  Lesson: even "need work" + "screens" routes normally via UI.

---

## B01-0C-012-002 — Knowledge-edge vs routable: discrimination examples

```
source_id: xiigen-b01-0c-012-002
source_title: XIIGen B01 knowledge-edge-012 — knowledge-edge discrimination examples
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

Discrimination table — knowledge-edge vs routable:

| Request | Predicate result | Routable? |
|---|---|---|
| "Map the navigation" | [] | NO — knowledge-edge |
| "Map the page navigation" | ["UI"] via "page" | YES → 1.1 |
| "Redesign the onboarding" | [] | NO — knowledge-edge |
| "Redesign the onboarding screen" | ["UI"] via "screen" | YES → 1.1 |
| "Improve the experience" | [] | NO — knowledge-edge |
| "Improve the ux experience" | ["UI"] via "ux" | YES → 1.1 |
| "Describe the flow" | ["DOCUMENT"] via "describe" | YES → 2.1 |
| "Document the flow" | ["DOCUMENT"] via "document" | YES → 2.1 |
| "Map the user flow" | [] | NO — "user" and "flow" not in lexicon |
| "Map the user interface flow" | ["UI"] via "interface" | YES → 1.1 |

---

## B01-0C-012-003 — Capability learning output for B01 gaps

```
source_id: xiigen-b01-0c-012-003
source_title: XIIGen B01 knowledge-edge-012 — capability learning schedule for B01 gaps
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

B01 gaps that should be scheduled for capability learning:

Gap ID: gap-navigation-no-predicate
  Description: "navigation" used alone without UI terms
  Recommended action: Consider adding "navigation" to UI lexicon in future
    batch, or extend compound detection to fire UI on "navigation + [structural noun]"

Gap ID: gap-ux-quality-language
  Description: "modern", "engaging", "feel", "improve" — UX quality adjectives
  Recommended action: Create a QUALITY predicate that routes to a quality review
    sub-phase (e.g., 1.6 UX Review)

Gap ID: gap-experience-without-ux
  Description: "user experience" in full form does not fire UI; only "ux" does
  Recommended action: Extend UI lexicon to include "user experience" as a phrase

Gap ID: gap-flow-standalone
  Description: "flow" alone (as in "user flow") does not fire any predicate
  Recommended action: Consider adding "flow" to UI lexicon as a compound with "user"

These gaps are flagged for wave-3 lexicon expansion. In B01, they remain
knowledge-edge cases with SCHEDULE_CAPABILITY_LEARNING recommended action.
