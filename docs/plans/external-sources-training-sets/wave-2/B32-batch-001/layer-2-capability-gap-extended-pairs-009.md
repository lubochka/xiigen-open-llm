# Layer 2 — Capability Gap Extended Pairs (009)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-L2-009-001 — "rough-cut architecture": ARCHITECTURE fires; rough-cut is gap warning

**Request:** "Design a rough-cut architecture for the feature flag system — nothing fancy."

**Layer 0-A trace:**
```
Token: "design" → ARCHITECTURE fires ✓
Token: "rough-cut" → gap-vibe-coding-rough-cut-001
Token: "architecture" → ARCHITECTURE fires ✓ (deduplicated with "design")
Token: "feature flag system" → domain noun
Token: "nothing fancy" → gap-vibe-coding-nothing-fancy-001

Predicates: ["ARCHITECTURE"]
GapWarnings: [gap-vibe-coding-rough-cut-001, gap-vibe-coding-nothing-fancy-001]
```

**Layer 0-B trace:**
```
Predicates: ["ARCHITECTURE"] → ArchitectureFlow31
DomainId: "software-development"
```

**Layer 1 trace:**
```
TransitionType: documentation
FlowOrder: [ArchitectureFlow31]
GapMetadata: [rough-cut-001, nothing-fancy-001]
```

**Layer 2 trace:**
```
CapabilityGapType: style_qualifier_present
GapWarningTypes: [rough_draft_framing, simplicity_framing]
KeyInsight: "rough-cut" and "nothing fancy" are quality qualifiers; ARCHITECTURE fires on "design" and "architecture" regardless
```

**Final result:** [ARCHITECTURE] → ArchitectureFlow31 + gap warnings

---

## B32-L2-009-002 — "rough-cut" in architecture context: extended analysis

**"rough-cut" signals:**
- The architecture produced will be high-level, not detailed
- Formal diagrams or specifications are not expected
- The output is a starting point, not a final deliverable

**System response:**
- ArchitectureFlow31 is still selected
- The rough-cut gap metadata may cause ArchitectureFlow31 to produce a lighter-weight artifact
- But the routing decision does not change

**Learning pipeline note:**
"rough-cut" is a recognized gap ID (gap-vibe-coding-rough-cut-001). No learning action is needed. The metadata informs downstream quality expectations.

---

## B32-L2-009-003 — "nothing fancy" in ARCHITECTURE context

**"nothing fancy" signals:**
- No complex design patterns required
- Simple, straightforward architecture preferred
- Minimal documentation expected

**Combined with "rough-cut":**
Both gap IDs are emitted. They reinforce each other — both indicate low formality expectation. The combined signal says: "produce a simple, rough architecture, not a detailed enterprise-grade design."

The routing is unchanged: ArchitectureFlow31 is still selected. The metadata informs the artifact quality expectations.

---
