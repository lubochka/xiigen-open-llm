# Layer 0-D — Flow Factory Pairs (036)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-036-001 — Round 3 factory reference: all qualifier-to-factory paths

**Factory bypass (knowledge_edge) for all Round 3 qualifiers alone:**

Any Round 3 qualifier (expedient, pragmatic, lean, barebones, stripped-down, no-frills, agile, iterative, just get it working, clean up later, wing it, whip up without object) without action verb:
→ no factory selected
→ knowledge_edge
→ SCHEDULE_CAPABILITY_LEARNING

---

## B32-0D-036-002 — Factory selection for all predicates: consolidated reference

**Predicate-to-factory mapping (unchanged from prior files):**

| Predicate | Factory | Flow |
|---|---|---|
| IMPLEMENT | ImplementationFactory | ImplementationFlow |
| UI | UIFactory | UIFlow |
| ARCHITECTURE | ArchitectureFactory | ArchitectureFlow31 |
| API_CONTRACT | APIContractFactory | APIContractFlow |
| SECURITY | SecurityFactory | SecurityFlow |
| TEST | TestFactory | TestFlow |
| DOCUMENT | DocumentFactory | DocumentFlow |
| QA | QAFactory | QAFlow |
| [] | none | none → knowledge_edge |

---

## B32-0D-036-003 — GapMetadata structure reference

**GapMetadata fields set when predicates are non-empty:**

```json
{
  "gapWarnings": ["<gap-id-1>", "<gap-id-2>"],
  "capabilityGapWarning": "style_qualifier_noted",
  "domainId": "software-development"
}
```

This metadata is applied to every factory flow created. No factory discards it.

---

## B32-0D-036-004 — DomainId invariant in factory output

**Factory output always includes DomainId = "software-development"**

Whether one factory or four factories are invoked, each flow carries DomainId = "software-development".
This value is never null and never "project-management".

---
