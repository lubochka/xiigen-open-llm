# Layer 3 — Teaching Unit Pairs (016)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-016-001 — "hack together" family: three synonyms, one gap ID

**Teaching unit:** "hack together", "cobble together", and "throw together" all describe casual, improvised assembly. All three map to gap-vibe-coding-hack-001.

**Core rule:**
```
"hack together" → gap-vibe-coding-hack-001
"cobble together" → gap-vibe-coding-hack-001 (same ID)
"throw together" → gap-vibe-coding-hack-001 (same ID)
"hack" (as standalone development verb) → gap-vibe-coding-hack-001
None of these fire IMPLEMENT
```

---

## B32-L3-016-002 — Examples: hack family alone vs with action verb

**Zero-predicate (hack family alone):**

| Request | Predicates | Gap ID |
|---|---|---|
| "Hack together a solution" | [] | gap-hack-001 |
| "Cobble together something" | [] | gap-hack-001 |
| "Throw together a prototype" | [] | gap-hack-001 + gap-prototype-001 |

**Predicate-firing (hack family + action verb):**

| Request | Action verb | Predicate + Gap |
|---|---|---|
| "Hack together + build" | build | [IMPLEMENT] + gap-hack-001 |
| "Cobble together + write" | write | [IMPLEMENT] + gap-hack-001 |
| "Throw together + implement" | implement | [IMPLEMENT] + gap-hack-001 |

---

## B32-L3-016-003 — "hack" in security context: different classification

**Important boundary:** "hack" in a security-threat context is not gap vocabulary.

- "Hack together a solution" → gap-vibe-coding-hack-001 (development methodology)
- "Prevent hackers from breaking in — secure the auth" → SECURITY fires on "secure"; "hackers" is a security threat noun

The key distinction: is "hack" being used as a development methodology verb, or as a security threat noun?

- Development methodology use → gap-vibe-coding-hack-001
- Security threat context → SECURITY predicate vocabulary (not gap)

---
