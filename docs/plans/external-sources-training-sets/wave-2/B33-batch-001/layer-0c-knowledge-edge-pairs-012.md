# Layer 0-C — Knowledge Edge Pairs (012)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-012-001 — Verb priority hierarchy for B33

Priority order (top = highest):
1. security vocabulary → SECURITY (prompt injection, sanitize, XSS analog)
2. architecture verbs → ARCHITECTURE (design, architect, structure)
3. integration verb → API_CONTRACT (integrate always)
4. documentation verb → DOCUMENT (document)
5. test-writing verbs → TEST (write/create/add tests)
6. code-writing verbs → IMPLEMENT (build, implement, create, develop, write code)
7. AI vocabulary without code verb → gap

Rule: When multiple verbs present, fire all applicable predicates. Code verb overrides AI object.

---

## KE-012-002 — Edge: "adapt" vs "implement adaptive"

"adapt the agent" → gap ("adapt" AI sense)
"implement adaptive routing" → IMPLEMENT ("implement" code verb fires)
"design adaptive architecture" → ARCHITECTURE
"build an adaptive learning module" → IMPLEMENT

Teaching: The verb form of "adapt" (AI configuration/behavior adaptation) → gap. Code verbs applied to adaptive features → predicates fire.

---

## KE-012-003 — Edge: "autonomous" as adjective does not suppress code verbs

"build an autonomous deployment pipeline" → IMPLEMENT ("build" fires despite "autonomous")
"design autonomous scaling architecture" → ARCHITECTURE
"implement autonomous retry logic" → IMPLEMENT

Teaching: "autonomous" as adjective modifying a software feature does not suppress predicate firing. Gap only when "autonomous" describes AI agent behavior without code verb.

---

## KE-012-004 — Edge: "orchestration" as noun

"agent orchestration" (noun, no verb) → gap
"orchestration layer" (noun, no code verb) → gap
"design the orchestration layer" → ARCHITECTURE (design verb fires)
"implement the orchestration service" → IMPLEMENT (implement fires)
"the orchestration is not working" → gap (observation)

---

## KE-012-005 — Edge: "AI" as adjective does not suppress predicates

"build an AI-powered recommendation engine" → IMPLEMENT ("build" fires)
"design an AI-driven analytics platform" → ARCHITECTURE ("design" fires)
"implement AI-based fraud detection" → IMPLEMENT + SECURITY (implement + security context)

Teaching: "AI-powered", "AI-driven", "AI-based" as adjectives do not gap-ify code verbs. The verb fires the predicate.
