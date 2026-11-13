# Layer 0-A — Compiler Pairs (012)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-012-001 — Complete B32 predicate/gap vocabulary table

**Purpose:** Reference table showing every B32-relevant token, its classification, and the resulting predicate or gap ID.

### Action Predicate Triggers (fire predicates regardless of vibe framing)

| Token | Predicate |
|---|---|
| implement | IMPLEMENT |
| build | IMPLEMENT |
| create | IMPLEMENT |
| write | IMPLEMENT |
| code (exact or root) | IMPLEMENT |
| coding (via "code" root) | IMPLEMENT |
| design | ARCHITECTURE |
| architect | ARCHITECTURE |
| architecture (noun) | ARCHITECTURE |
| integrate | API_CONTRACT |
| API (noun) | API_CONTRACT |
| endpoint | API_CONTRACT |
| secure | SECURITY |
| security | SECURITY |
| test / unit test | TEST |
| document | DOCUMENT |
| QA | QA |

### UI Vocabulary (fire UI predicate)

| Token | Notes |
|---|---|
| screen | ✓ UI |
| page | ✓ UI |
| interface | ✓ UI |
| component | ✓ UI |
| form | ✓ UI |
| layout | ✓ UI |
| button | ✓ UI |
| input | ✓ UI |
| widget | ✓ UI |
| modal | ✓ UI |
| dashboard | ✗ NOT in UI lexicon → gap-vibe-coding-dashboard-001 |
| panel | ✗ NOT in UI lexicon |

### B32 Gap Vocabulary (emit gap warnings; NEVER fire predicates)

| Token | Gap ID |
|---|---|
| vibe, vibe code, vibe coding | gap-vibe-coding-style-001 |
| exploratory, explore | gap-vibe-coding-exploratory-001 |
| prototype (alone, no action verb) | gap-vibe-coding-prototype-001 |
| dashboard | gap-vibe-coding-dashboard-001 |
| spike | gap-vibe-coding-spike-001 |
| hack together, hack | gap-vibe-coding-hack-001 |
| demo (standalone) | gap-vibe-coding-demo-001 |
| rough-cut, rough cut | gap-vibe-coding-rough-cut-001 |
| intuition-driven, feel right, intuitive | gap-vibe-coding-intuition-001 |
| throwaway code, throwaway | gap-vibe-coding-throwaway-001 |
| quick and dirty | gap-vibe-coding-quick-dirty-001 |
| just make it work, just get it working | gap-vibe-coding-just-works-001 |
| nothing fancy | gap-vibe-coding-nothing-fancy-001 |
| scaffold (alone) | gap-vibe-coding-style-001 |
| throw together, cobble together | gap-vibe-coding-hack-001 |
| good enough | gap-vibe-coding-style-001 |
| creative mode | gap-vibe-coding-style-001 |

### Zero-Predicate Conditions → knowledge_edge

A request fires zero predicates when:
1. Only gap vocabulary is present with no registered action verbs
2. "prototype" appears alone (no action verb co-occurrence)
3. "spike" appears alone (no action verb co-occurrence)
4. "dashboard" is the only UI-like term (not in UI lexicon)

### DomainId for B32

Always: `"software-development"`
Never: `"project-management"` (that is B36/B37)
Never: `"vibe-coding"` (not a registered DomainId)

---

## B32-0A-012-002 — B32 result patterns summary

| Request pattern | Result | Notes |
|---|---|---|
| action verb + vibe framing | [predicate] + gap warnings | Predicate fires; framing is warning only |
| vibe vocabulary alone | [] | knowledge_edge + SCHEDULE_CAPABILITY_LEARNING |
| prototype + action verb | [IMPLEMENT] + gap-prototype-001 | Action verb determines predicate |
| prototype alone | [] | knowledge_edge |
| dashboard alone or with vibe | [] | dashboard not in UI lexicon |
| dashboard + screen | [UI] + gap-dashboard-001 | screen fires UI; dashboard still gap |
| coding (gerund) | [IMPLEMENT] | "code" root match |
| all 7 action verbs | [ARCHITECTURE, API_CONTRACT, IMPLEMENT, SECURITY, TEST, DOCUMENT, QA] | Vibe framing → warning only |

---
