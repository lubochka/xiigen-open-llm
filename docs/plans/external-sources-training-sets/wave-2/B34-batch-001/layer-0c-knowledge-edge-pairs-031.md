# Layer 0-C — Knowledge Edge Pairs (031)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-031-001 — "introduce" software insertion edge cases

"introduce microservices into the monolith" → IMPLEMENT (software insertion into system).
"introduce a caching layer" → IMPLEMENT (new software component).
"introduce retry logic into the client" → IMPLEMENT (code addition).
"introduce the team to microservices" → gap (person object, not software).
"introduce microservices to the team" → gap (teaching: microservices = topic, team = audience).

---

## KE-031-002 — "discuss" boundary with ARCHITECTURE

"discuss architecture" → gap (knowledge-sharing about architecture).
"discuss and then design the architecture" → ARCHITECTURE fires.
"discuss architectural options" → gap (options = decision-stage).
"discuss and select the architecture" = still gap if "select" is decision, not code action.
"discuss and architect the system" → ARCHITECTURE fires (architect = code action).

---

## KE-031-003 — guidance + "document" action knowledge edge

"explain the documentation" → gap (explain = always gap; topic = documentation).
"explain and document" → DOCUMENT fires from "document"; "explain" = gap warning.
"discuss the documentation needs" → gap.
"discuss and document the API" → DOCUMENT fires; "discuss" = gap warning.
"brief on documentation standards" → gap.
"brief on and then document" → DOCUMENT fires; "brief" = gap warning.

---

## KE-031-004 — "present" + "DOCUMENT" boundary

"present the documentation plan" → gap (present = teaching; plan = presentation topic).
"present and write the documentation" → DOCUMENT fires; "present" = gap warning.
"present the API documentation" → gap (presenting docs = teaching act, not doc-writing).
"review and update the documentation" → DOCUMENT fires; "review" = gap warning.

---

## KE-031-005 — third-party action + Round 2 vocabulary

"show [person] doing [action]" → gap (requester shows; person performs action).
"demonstrate [team] integrating" → gap warning for demonstrate + API_CONTRACT fires if requester integrates; if team integrates (third-party) → gap.
"run through [team member] building [feature]" → gap (their action; requester runs through).
Rule: only requester's actions fire predicates. Third-party actions in guided observation = gap.
