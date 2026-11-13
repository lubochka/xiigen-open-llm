# Layer 0-A — Compiler Pairs (029)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Cloudinary media connector

---

## B17-0A-029-001 — Cloudinary: build connector

**Input:** "Build the Cloudinary media upload connector with signed URL webhook callbacks."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |
| webhook | API_CONTRACT | 5 (reinforces; fires once) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Integration vocabulary: connector, webhook → IMPLEMENT routes to 4.3

---

## B17-0A-029-002 — Cloudinary: integrate

**Input:** "Integrate Cloudinary into the image processing pipeline."

Triggers: "Integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
"image processing pipeline" is a domain phrase — not integration vocabulary.

---

## B17-0A-029-003 — Cloudinary: build with token auth

**Input:** "Build Cloudinary SDK wrapper with API secret token validation."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| SDK | API_CONTRACT (reinforces to 4.3 routing) |
| token | SECURITY (Rule 6; unconditional) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)
"SDK" counts as integration vocabulary — routes IMPLEMENT to 4.3.

---

## B17-0A-029-004 — Cloudinary: implement (no connector vocabulary)

**Input:** "Implement Cloudinary image resizing service."

Triggers: "Implement" → IMPLEMENT
Integration vocabulary check: no connector, webhook, API, SDK, external service → IMPLEMENT routes to 4.1
CompiledPredicateIds: ["IMPLEMENT"]
DecisionCount: 1 | UniqueSubPhases: 1 (4.1)
"Cloudinary" proper noun does not supply integration vocabulary.

---

## B17-0A-029-005 — Cloudinary: build with test and QA

**Input:** "Build Cloudinary connector, write upload tests, QA the delivery pipeline."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| write upload tests | TEST |
| QA | QA (Rule 21; abbreviation only) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST", "QA"]
DecisionCount: 4 | UniqueSubPhases: 4 (4.3, 3.3, 5.1, 7.x)
QAGatingFlow73 is last-absolute.
