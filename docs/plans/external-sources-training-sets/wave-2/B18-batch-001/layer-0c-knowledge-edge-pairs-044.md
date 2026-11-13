# Layer 0-C — Knowledge-Edge Pairs (044)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-044-001 — Multi-predicate edge interaction: IMPLEMENT + SECURITY + TEST

**When IMPLEMENT + SECURITY + TEST fire together:**

| Edge combination | Output | Notes |
|---|---|---|
| "write migration" + "PII" + "write tests" | IMPLEMENT + SECURITY + TEST | Three predicates, three flows |
| "anonymize" + "GDPR" + "write integration tests" | IMPLEMENT + SECURITY + TEST | Same result |
| "mask credit cards" + "write tests" | IMPLEMENT + SECURITY + TEST | "mask" = IMPLEMENT + SECURITY |
| "migrate PII data" + "add tests" | IMPLEMENT + SECURITY + TEST | All three |

**Execution order edge:** DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51

---

## B18-0C-044-002 — Multi-predicate edge interaction: IMPLEMENT + ARCHITECTURE + SECURITY

**When IMPLEMENT + ARCHITECTURE + SECURITY fire together:**

| Edge combination | Output |
|---|---|
| "design" + "structure" + "PII" + "write migration" | IMPLEMENT + ARCHITECTURE + SECURITY |
| "design pattern" + "GDPR-compliant" + "implement" | IMPLEMENT + ARCHITECTURE + SECURITY |
| "architecture" + "encrypted" + "create migration" | IMPLEMENT + ARCHITECTURE + SECURITY |

**Execution order edge:** ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow

---

## B18-0C-044-003 — Multi-predicate edge interaction: refactoring verb + ARCHITECTURE token

**When refactoring verb AND ARCHITECTURE token co-occur:**

| Phrase | Predicates | TransitionType |
|---|---|---|
| "consolidate migrations into pattern" | IMPLEMENT + ARCHITECTURE | refactoring |
| "restructure schema structure" | IMPLEMENT + ARCHITECTURE | refactoring |
| "rename column, design FK pattern" | IMPLEMENT + ARCHITECTURE | refactoring |
| "consolidate shard key pattern" | IMPLEMENT + ARCHITECTURE | refactoring |
| "refactor audit tables and design audit architecture" | IMPLEMENT + ARCHITECTURE | refactoring |

**Key edge:** Refactoring verb sets TransitionType = refactoring even when ARCHITECTURE also fires. The predicate set and TransitionType are independent: predicates list what to DO; TransitionType describes HOW the operation is classified.

---

## B18-0C-044-004 — Edge disambiguation: "pattern" co-occurrence with DOCUMENT

**When "document the X pattern" appears:**

| Analysis | Result |
|---|---|
| DOCUMENT fires | YES — "document" is verb (Rule 4) |
| ARCHITECTURE fires | YES — "pattern" is noun (Rule 7, absolute) |
| IMPLEMENT fires | Only if "write migration" or similar also present |
| Both predicates in output | YES |
| Execution order | ArchitectureFlow31 precedes DataMigrationFlow44 (if IMPLEMENT present) |
| DocumentationFlow21 | Parallel-eligible |

**This is the "document the X pattern" canonical edge case. Both ARCHITECTURE and DOCUMENT predicates are always present. No suppression.**

---

## B18-0C-044-005 — Edge reference: complete v6-04 detection edges

**v6-04 (SECURITY suppressed for anonymization/masking vocabulary) detection edges:**

| Token present | SECURITY should fire? | v6-04 error if absent |
|---|---|---|
| "anonymize" (verb) | YES | SECURITY missing |
| "mask" (verb, PII context) | YES | SECURITY missing |
| "PII" | YES | SECURITY missing |
| "GDPR" | YES | SECURITY missing |
| "encrypted" | YES | SECURITY missing |
| "sensitive data" | YES | SECURITY missing |
| "row-level security" | YES | SECURITY missing |
| "TLS" | YES | SECURITY missing |
| "credentials" | YES | SECURITY missing |
| "access control" | YES | SECURITY missing |

**v6-04 detection rule:** If output predicates contain IMPLEMENT (from "anonymize"/"mask"/"write migration with PII") but do NOT contain SECURITY despite the above tokens being present — this is a v6-04 error. Add SECURITY predicate.
