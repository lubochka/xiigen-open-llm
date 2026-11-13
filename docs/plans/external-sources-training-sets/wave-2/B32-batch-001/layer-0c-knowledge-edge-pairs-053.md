# Layer 0-C — Knowledge-Edge Pairs (053)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-053-001 — complete gap vocabulary knowledge-edge: all six gap ID families

**Knowledge edge: any of the six gap ID families alone → SCL**

| Gap ID family | Zero-predicate examples |
|---|---|
| gap-vibe-coding-style-001 | "lean approach", "be pragmatic", "expedient solution" |
| gap-vibe-coding-prototype-001 | "just a prototype", "run a PoC", "wireframe it" |
| gap-vibe-coding-spike-001 | "technical spike", "walking skeleton", "thin slice" |
| gap-vibe-coding-rough-cut-001 | "rough cut only", "rough draft" |
| gap-vibe-coding-hack-001 | "let's hack something together", "cobble it together" |
| gap-vibe-coding-dashboard-001 | "dashboard concept" (no action verb) |

All → predicates = [] → knowledge_edge + SCL (once)

---

## B32-0C-053-002 — complete predicate trigger reference

**Knowledge edge: predicate trigger summary**

| Predicate | Fired by |
|---|---|
| IMPLEMENT | implement, build, create, write (code), code, construction noun |
| UI | screen, interface, UI (with action verb) |
| ARCHITECTURE | architect, design (arch context), structure, plan (arch context) |
| API_CONTRACT | define contract, specify API, document contract |
| SECURITY | secure, audit (security context) |
| TEST | test, write tests, unit test, bang out tests |
| DOCUMENT | document, write docs, create docs |
| QA | QA, quality assurance, run QA |

---

## B32-0C-053-003 — knowledge-edge: noun forms that do NOT fire predicates

**Knowledge edge: noun forms that look like predicates but aren't**

| Noun form | Predicate fires? | Why not |
|---|---|---|
| "architecture" | NO | Noun; "architect" fires |
| "implementation" | YES | In IMPLEMENT lexicon |
| "testing" (alone) | NO | Gerund without imperative |
| "documentation" (alone) | NO | Noun; "document" fires |
| "security" (alone) | NO | Noun; "secure" fires |
| "API contract" (alone) | NO | Noun phrase; "define" fires |
| "QA" (as org noun) | NO | "QA the X" fires; "QA team" doesn't |

---

## B32-0C-053-004 — complete B32 rule set (final summary)

**All B32 rules in one place:**

1. Gap vocabulary alone → knowledge_edge → SCL (once)
2. Gap vocabulary + action verb → predicate fires + gap warning (no SCL)
3. Style qualifiers never suppress predicates
4. "dashboard" never fires UI (always gap-vibe-coding-dashboard-001)
5. "vibe code" contains "code" root → IMPLEMENT fires
6. SCL emits once per request
7. DomainId = "software-development" always
8. Negated vocabulary does not fire its predicate
9. Gap ID must match actual vocabulary
10. No gap vocabulary → no SCL, no gap warning

---
