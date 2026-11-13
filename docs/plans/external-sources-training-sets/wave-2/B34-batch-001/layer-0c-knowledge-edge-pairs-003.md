# Layer 0-C — Knowledge Edge Pairs (003)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-003-001 — "create" noun disambiguation: DOCUMENT vs IMPLEMENT

"create a [tutorial/guide/README/runbook/playbook/spec/wiki page/article/course]" → DOCUMENT
"create a [system/module/service/platform/tool/tracker/portal/application]" → IMPLEMENT
"create a [training program/curriculum/learning path]" → DOCUMENT (content artifact default)
"create a [training program management system]" → IMPLEMENT (software artifact with system/management noun)

Rule: The noun type determines routing. Documentation artifact nouns → DOCUMENT. Software component nouns → IMPLEMENT.

---

## KE-003-002 — "design" noun disambiguation: ARCHITECTURE vs gap

"design a [system/architecture/platform/layer/service/infrastructure]" → ARCHITECTURE
"design a [training program/curriculum/course/workshop]" → gap (curriculum design vocabulary)
"design a [database schema/API contract]" → ARCHITECTURE (structural design artifact)

Rule: "design" fires ARCHITECTURE only with structural/software artifact nouns, not curriculum/event nouns.

---

## KE-003-003 — "explain" never fires any predicate

"explain the authentication flow" → gap (not SECURITY)
"explain the API design" → gap (not API_CONTRACT, not DOCUMENT)
"explain how to write tests" → gap (not TEST)
"explain the architecture" → gap (not ARCHITECTURE)

Rule: "explain" is always gap vocabulary regardless of what follows. No predicate fires for explanation requests.

DomainId for all B34 gaps: software-development. No "education" or "training" domain exists.
