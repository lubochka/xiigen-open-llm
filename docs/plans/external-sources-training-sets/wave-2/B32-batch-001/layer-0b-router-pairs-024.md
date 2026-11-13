# Layer 0-B — Router Pairs (024)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-024-001 — Complete gap vocabulary routing summary

**Reference: all gap contexts and their routing outcomes**

| Gap context | Predicates also present? | Route |
|---|---|---|
| gap-vibe-coding-style-001 only | No | knowledge_edge |
| gap-vibe-coding-spike-001 only | No | knowledge_edge |
| gap-vibe-coding-prototype-001 only | No | knowledge_edge |
| gap-vibe-coding-dashboard-001 only | No | knowledge_edge |
| gap-vibe-coding-hack-001 only | No | knowledge_edge |
| gap-vibe-coding-throwaway-001 only | No | knowledge_edge |
| gap-vibe-coding-rough-cut-001 only | No | knowledge_edge |
| gap-vibe-coding-quick-dirty-001 only | No | knowledge_edge |
| gap-vibe-coding-just-works-001 only | No | knowledge_edge |
| Any gap + IMPLEMENT | Yes | ImplementationFlow + gap warning |
| Any gap + UI | Yes | UIFlow + gap warning |
| Any gap + ARCHITECTURE | Yes | ArchitectureFlow31 + gap warning |
| Any gap + TEST | Yes | TestFlow + gap warning |
| Any gap + DOCUMENT | Yes | DocumentFlow + gap warning |
| Any gap + API_CONTRACT | Yes | APIContractFlow + gap warning |
| Any gap + QA | Yes | QAFlow + gap warning |

---

## B32-0B-024-002 — SCHEDULE_CAPABILITY_LEARNING emission rule

**Rule:** SCHEDULE_CAPABILITY_LEARNING is emitted if and only if predicates = [].

| Predicates | SCHEDULE_CAPABILITY_LEARNING |
|---|---|
| [] | Emitted once |
| [IMPLEMENT] | NOT emitted |
| [UI] | NOT emitted |
| [ARCHITECTURE] | NOT emitted |
| [IMPLEMENT, UI] | NOT emitted |
| any non-empty list | NOT emitted |

**Gap warnings are emitted regardless of predicate presence.** They appear in the output whether predicates are empty or not. Only SCHEDULE_CAPABILITY_LEARNING is gated on empty predicates.

**DomainId:** Always "software-development" for B32.

---
