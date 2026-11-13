# Layer 0-B — Router Pairs (056)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-056-001 — "program increment" review routing edge case

**Input:** "Review the program increment and validate SAFe adoption criteria."

Token scan:
- "Review" → QA fires
- "program increment" → gap
- "validate" → QA fires (already fired)
- "SAFe adoption" → gap
- "criteria" → DOCUMENT fires

Predicates: ["QA", "DOCUMENT"]
Routes: QA sub-phase + DocumentationFlow (2.1)
DomainId: "software-development"

---

## B37-0B-056-002 — "portfolio management" structure routing edge case

**Input:** "Design the portfolio management governance architecture and document it for stakeholders."

Token scan:
- "Design" → ARCHITECTURE fires
- "portfolio management" → gap
- "governance" → gap
- "architecture" → ARCHITECTURE fires (already fired)
- "document" → DOCUMENT fires
- "stakeholders" → DOCUMENT fires (already fired)

Predicates: ["ARCHITECTURE", "DOCUMENT"]
Routes: ArchitectureFlow31 (3.1) + DocumentationFlow (2.1)
DomainId: "software-development"

---

## B37-0B-056-003 — "value stream" + API token edge case

**Input:** "Map value stream API dependencies between services."

Token scan:
- "Map" → gap
- "value stream" → gap
- "API dependencies" → "API" + "dependencies" → API_CONTRACT fires
- "between services" → "services" → IMPLEMENT fires

Predicates: ["API_CONTRACT", "IMPLEMENT"]
Routes: APIContractFlow33 (3.3) + BackendServiceFlow41 (4.1)
DomainId: "software-development"

---

## B37-0B-056-004 — "agile transformation" + API token edge case

**Input:** "Coordinate agile transformation and document the API contract for services."

Token scan:
- "Coordinate" → gap; "agile transformation" → gap
- "document" → DOCUMENT fires
- "API contract" → API_CONTRACT fires
- "services" → IMPLEMENT fires

Predicates: ["DOCUMENT", "API_CONTRACT", "IMPLEMENT"]
Routes: DocumentationFlow (2.1) + APIContractFlow33 (3.3) + BackendServiceFlow41 (4.1)

---

## B37-0B-056-005 — Routing: API_CONTRACT in PM managing context

**Rule confirmed:** API_CONTRACT fires when "dependencies" + API co-occurrence token appear, regardless of PM managing context.

Example: "Map SAFe PI dependencies between API services." → API_CONTRACT + IMPLEMENT fires.
PM managing vocabulary (SAFe, PI) → gap warnings only.
The route is 3.3 + 4.1, not knowledge_edge.
