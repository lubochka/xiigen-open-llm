# Layer 3 — Teaching Unit Pairs (012)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-012-001 — Complete B32 teaching summary

**Teaching unit:** Consolidated reference for all B32 rules and their rationale.

### Rule 1: Prototype requires action verb
- "prototype" alone → gap → knowledge_edge
- "build/create/write/implement/code + prototype" → IMPLEMENT + gap warning
- **Why:** "prototype" is a noun (artifact type); the system routes on verbs (actions)

### Rule 2: Dashboard not in UI lexicon
- "dashboard" → gap-vibe-coding-dashboard-001
- UI fires on: screen, page, interface, component, form, layout, button, input, widget, modal
- **Why:** "dashboard" is a composite concept, not a primitive UI element

### Rule 3: Style qualifiers never suppress predicates
- All B32 vibe vocabulary → gap warnings only
- Action verbs fire predicates unconditionally
- **Why:** Routing is action-based; style is metadata

### Rule 4: B32 DomainId = "software-development"
- Always: software-development
- Never: project-management, vibe-coding, or invented values
- **Why:** Vibe coding is software development with unrecognized style vocabulary

### Rule 5: Spike/hack/demo/throwaway are all gap
- None of these fire any registered predicate
- All produce gap warnings
- **Why:** They are informal methodology vocabulary without predicate-lexicon entries

### Rule 6: All vibe forms are gap
- vibe, vibe code, vibe coding, vibe out, creative mode → gap-vibe-coding-style-001
- **Exception:** "code" root within "vibe code" fires IMPLEMENT
- **Why:** "vibe" has no predicate mapping; "code" has IMPLEMENT mapping

### Rule 7: "coding" fires IMPLEMENT via root match
- "coding" → "code" root → IMPLEMENT fires
- Applies in: "exploratory coding", "vibe coding", "rough coding"
- **Why:** Root matching for gerunds extends predicate coverage to gerund forms

### Rule 8: SCHEDULE_CAPABILITY_LEARNING fires once when predicates = []
- One emission per request regardless of gap ID count
- DomainId: "software-development"
- **Why:** One learning signal per unresolved request; bundle all gaps

### Rule 9: v6-01 (VIBE_CODING invented) is always wrong
- VIBE_CODING is not a registered predicate
- **Correction:** route on "code" root (IMPLEMENT) or emit gap warning

### Rule 10: v6-02 (predicate suppression) is always wrong
- Style framing never cancels action predicates
- **Correction:** fire the predicate + emit the gap warning

### Rule 11: v6-03 (wrong DomainId) is always wrong
- B32 never uses DomainId "project-management"
- **Correction:** always "software-development"

---

## B32-L3-012-002 — Quick reference: B32 decision flowchart

```
1. Scan for action verbs (implement/build/create/write/code)
   YES → fire IMPLEMENT; record gap vocabulary as warnings; route to BackendServiceFlow41
   NO  → continue

2. Scan for UI vocabulary (screen/page/interface/component/form/layout/button/input/widget/modal)
   YES → fire UI; record gap vocabulary as warnings; route to UIFlow
   NO  → continue

3. Scan for ARCHITECTURE vocabulary (design/architect/architecture)
   YES → fire ARCHITECTURE; route to ArchitectureFlow31
   NO  → continue

4. Scan for other predicate lexicons (API_CONTRACT/SECURITY/TEST/DOCUMENT/QA)
   YES → fire respective predicates; route to respective flows
   NO  → continue

5. No predicates fired → knowledge_edge + SCHEDULE_CAPABILITY_LEARNING (DomainId: software-development)

Throughout: any B32 gap vocabulary → emit corresponding gap-vibe-coding-XXX-001 warning
```

---
