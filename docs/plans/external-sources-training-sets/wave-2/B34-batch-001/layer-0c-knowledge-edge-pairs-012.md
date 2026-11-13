# Layer 0-C — Knowledge Edge Pairs (012)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-012-001 — Complete B34 verb priority hierarchy

| Priority | Verb(s) | Predicate | B34 Notes |
|----------|---------|-----------|-----------|
| 1 | design/architect/plan (+ structural noun) | ARCHITECTURE | curriculum nouns → gap instead |
| 2 | integrate/connect | API_CONTRACT | always fires in guidance context too |
| 3 | build/implement/create (+ software noun) | IMPLEMENT | documentation nouns → DOCUMENT instead |
| 4 | secure/harden/protect | SECURITY | teach-about-security → gap |
| 5 | authorize/add RBAC | AUTHORIZATION | — |
| 6 | write/create tests | TEST | TDD/BDD alone → gap |
| 7 | write/document/create (+ doc noun) | DOCUMENT | explain ≠ DOCUMENT |
| 8 | QA (abbreviation) | QA | "quality assurance" spelled out → gap |
| gap | mentor/coach/teach/train/guide/explain/onboard/etc. | [] | all guidance vocabulary |

---

## KE-012-002 — B34 gap summary vs B33 gap summary

B34 (skills/guidance gap): Human learning vocabulary → gap
- Gap IDs: gap-skills-guidance-mentor-001, gap-skills-guidance-onboard-001, gap-skills-guidance-train-001

B33 (AI agent gap): AI/ML vocabulary → gap
- Gap IDs: gap-ai-agent-adapt-001, gap-ai-agent-skill-001, gap-ai-agent-llm-001

Both batches: DomainId = software-development. SCHEDULE_CAPABILITY_LEARNING emitted. GapHandler response identical.

---

## KE-012-003 — "knowledge base" disambiguation

"Build the knowledge base system" → IMPLEMENT (build + software system)
"Write knowledge base articles" → DOCUMENT (write + documentation artifact)
"Share knowledge base entries" → gap (share = knowledge transfer vocabulary)
"Integrate the knowledge base with Confluence" → API_CONTRACT (integrate always fires)
