# Layer 0-B — Router Pairs (022)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-022-001 — [ARCHITECTURE] → ARCHITECTURE: "rough architecture sketch"

**Predicates received:** `["ARCHITECTURE"]`

**Gap context:** none (rough alone does not produce a registered gap ID)

**Router decision:**

- ARCHITECTURE → ArchitectureFlow31
- No gap warning produced

**Output:**
```
RouteTarget: ArchitectureFlow31
FlowSelected: ArchitectureFlow31
DomainId: "software-development"
GapWarnings: []
```

---

## B32-0B-022-002 — [ARCHITECTURE] → ARCHITECTURE: "scrappy architecture design"

**Predicates received:** `["ARCHITECTURE"]`

**Gap context:** gap-vibe-coding-style-001 ("scrappy")

**Router decision:**

- ARCHITECTURE → ArchitectureFlow31
- Gap warning logged; routing unaffected

**Output:**
```
RouteTarget: ArchitectureFlow31
FlowSelected: ArchitectureFlow31
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0B-022-003 — [ARCHITECTURE, API_CONTRACT] → dual flow: "quick and dirty architecture + API"

**Predicates received:** `["ARCHITECTURE", "API_CONTRACT"]`

**Gap context:** gap-vibe-coding-quick-dirty-001

**Router decision:**

- ARCHITECTURE → ArchitectureFlow31
- API_CONTRACT → APIContractFlow
- Both selected; gap warning noted

**Output:**
```
RouteTarget: [ArchitectureFlow31, APIContractFlow]
FlowSelected: [ArchitectureFlow31, APIContractFlow]
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-quick-dirty-001]
```

---

## B32-0B-022-004 — [ARCHITECTURE, IMPLEMENT] → dual flow: "hack together + design + build"

**Predicates received:** `["ARCHITECTURE", "IMPLEMENT"]`

**Gap context:** gap-vibe-coding-hack-001, gap-vibe-coding-style-001

**Router decision:**

- ARCHITECTURE → ArchitectureFlow31; IMPLEMENT → ImplementationFlow
- Both gap warnings noted; routing selects both flows

**Output:**
```
RouteTarget: [ArchitectureFlow31, ImplementationFlow]
FlowSelected: [ArchitectureFlow31, ImplementationFlow]
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-hack-001, gap-vibe-coding-style-001]
```

---
