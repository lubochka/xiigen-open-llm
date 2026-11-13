# Cross-Layer Synthesis (003)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: cross-layer-synthesis

---

## B37-CLS-003-001 — "dependencies" context rule: side-by-side comparison trace

**Two requests — same structure, different outcomes:**

**Request A:** "Document the API dependencies for the payment service endpoint."
**Request B:** "Document the project team dependencies for the payment delivery programme."

---

## B37-CLS-003-002 — Request A trace: API_CONTRACT fires

**Request A:** "Document the API dependencies for the payment service endpoint."

### Layer 0-A (Compiler):
- "Document" → DOCUMENT fires (Rule 8)
- "API" → API co-occurrence token
- "dependencies" → API_CONTRACT fires (API co-occurrence: "API", "endpoint")
- "payment" → not in predicate lexicon
- "service" → IMPLEMENT fires (Rule 9)
- "endpoint" → API co-occurrence token

Predicate list: ["API_CONTRACT", "DOCUMENT", "IMPLEMENT"]

### Layer 0-B (Router):
- API_CONTRACT → 3.3
- DOCUMENT → 2.1
- IMPLEMENT → 4.1
- DomainId: "software-development"

### Layer 0-D (Flow Factory):
- APIContractFlow33 + DocumentationFlow + BackendServiceFlow41

### Layer 1 (Orchestrator):
- TransitionType: api_contract + documentation + feature_addition
- Parallel eligible: all three

**Output A:**
```
Predicates: ["API_CONTRACT", "DOCUMENT", "IMPLEMENT"]
DomainId: "software-development"
Routes: 3.3 + 2.1 + 4.1
Gap IDs: none
```

---

## B37-CLS-003-003 — Request B trace: gap only (no API_CONTRACT)

**Request B:** "Document the project team dependencies for the payment delivery programme."

### Layer 0-A (Compiler):
- "Document" → DOCUMENT fires (Rule 8)
- "project team" → not in any predicate lexicon
- "dependencies" → no API co-occurrence tokens present → gap (Rule 5)
- "payment" → not in predicate lexicon
- "delivery" → gap
- "programme" → not in predicate lexicon

Predicate list: ["DOCUMENT"]

### Layer 0-B (Router):
- DOCUMENT → 2.1
- DomainId: "software-development"

### Layer 0-C (Knowledge Edge):
- "dependencies" without API co-occurrence → gap-project-managing-dependencies-team-001
- "delivery" → gap-project-managing-manage-delivery-001

### Layer 0-D (Flow Factory):
- DocumentationFlow only (no APIContractFlow33 — no API co-occurrence)

### Layer 1 (Orchestrator):
- TransitionType: documentation
- Gap IDs forwarded

**Output B:**
```
Predicates: ["DOCUMENT"]
DomainId: "software-development"
Routes: 2.1
Gap IDs: [gap-project-managing-dependencies-team-001, gap-project-managing-manage-delivery-001]
```

---

## B37-CLS-003-004 — Side-by-side comparison

| Aspect | Request A (API dependencies) | Request B (team dependencies) |
|--------|------------------------------|-------------------------------|
| API co-occurrence | "API" + "endpoint" present | None |
| API_CONTRACT | FIRES | does NOT fire |
| DOCUMENT | fires | fires |
| IMPLEMENT | fires (service) | does NOT fire |
| Predicate count | 3 | 1 |
| Routes | 3.3 + 2.1 + 4.1 | 2.1 only |
| Gap IDs | none | dependencies-team-001 + manage-delivery-001 |

**Key lesson:** The word "dependencies" alone is not sufficient to fire API_CONTRACT. The presence or absence of API co-occurrence tokens is the deciding factor.

---

## B37-CLS-003-005 — Corruption risk for this trace

**Most common corruption:** Firing API_CONTRACT on Request B because it contains "dependencies"

**Correct behaviour:** No API co-occurrence tokens in Request B → "dependencies" is gap → no API_CONTRACT

**Test:** "What API co-occurrence tokens are present in 'Document the project team dependencies for payment delivery programme'?"
Answer: None. "project", "team", "payment", "delivery", "programme" are all non-API-co-occurrence tokens.
Therefore: "dependencies" → gap-project-managing-dependencies-team-001.
