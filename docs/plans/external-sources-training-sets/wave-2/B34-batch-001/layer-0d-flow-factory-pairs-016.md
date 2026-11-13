# Layer 0-D — Flow Factory Pairs (016)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-016-001 — "Workshop" facilitation: factory map

| Request | Predicates | Factory | TransitionType |
|---------|-----------|---------|----------------|
| "Run a workshop on clean code" | [] | GapHandler | none |
| "Facilitate a workshop on TDD" | [] | GapHandler | none |
| "Design a workshop curriculum" | [] | GapHandler | none |
| "Build a workshop management platform" | [IMPLEMENT] | ImplementationFlow41 | feature_addition |
| "Write a workshop agenda and materials" | [DOCUMENT] | DocumentationFlow | documentation |
| "Workshop + participants build demo service" | [] | GapHandler | none |

---

## FF-016-002 — Workshop + participant actions → GapHandler (third-party actions)

"Run a workshop where the team implements a microservice" → GapHandler
"Facilitate a hands-on workshop where engineers write E2E tests" → GapHandler
"Hold a session where participants integrate their APIs" → GapHandler (normally integrate fires, but "where participants integrate" = third-party action; facilitate = requester's action = gap)

Exception: "Facilitate the integration of A with B" → API_CONTRACT fires.
Distinction: "facilitate integration" (noun phrase = structural coupling) vs "facilitate [session] where participants integrate" (third-party action in session).

---

## FF-016-003 — Workshop curriculum vs workshop platform: noun type factory decision

"Design workshop curriculum" → gap (curriculum = content noun)
"Design the workshop scheduling system" → ARCHITECTURE (system = structural noun)
"Build the workshop registration portal" → IMPLEMENT (build + software portal)
"Write the workshop participant guide" → DOCUMENT (write + guide = documentation artifact)
"QA the workshop enrollment workflow" → QA (QA abbreviation fires)
