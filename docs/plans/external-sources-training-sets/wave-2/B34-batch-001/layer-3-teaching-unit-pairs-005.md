# Layer 3 — Teaching Unit Pairs (005)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-005 — "create" noun type determines DOCUMENT vs IMPLEMENT

**Rule**: "create" fires DOCUMENT when followed by a documentation artifact noun, and IMPLEMENT when followed by a software component noun.

**Documentation artifacts → DOCUMENT**:
- guide, tutorial, README, runbook, playbook, spec, article, course, curriculum, learning path, checklist (as document), manual, wiki page, knowledge base article

**Software components → IMPLEMENT**:
- system, module, service, platform, tool, tracker, portal, application, dashboard, engine, registry, bot, API

**Test artifacts → TEST**:
- test suite, unit tests, integration tests, E2E tests

**Examples**:

| "create a [noun]" | Predicate |
|-------------------|-----------|
| create a tutorial | DOCUMENT |
| create a module | IMPLEMENT |
| create a README | DOCUMENT |
| create a service | IMPLEMENT |
| create a test suite | TEST |
| create a platform | IMPLEMENT |
| create a learning path | DOCUMENT (content default) |
| create a learning path tracker | IMPLEMENT (software artifact suffix) |

**Key disambiguation signal**: If the noun ends in "system/platform/service/tool/tracker/engine", → IMPLEMENT. If it ends in "guide/article/spec/tutorial/path/course", → DOCUMENT.
