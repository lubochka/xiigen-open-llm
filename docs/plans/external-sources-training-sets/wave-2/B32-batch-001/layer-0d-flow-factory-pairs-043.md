# Layer 0-D — Flow Factory Pairs (043)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-043-001 — complete factory reference (final)

**All factories, their predicates, and their flows:**

| Predicate | Factory | Flow name | TransitionType contribution |
|---|---|---|---|
| IMPLEMENT | ImplementationFactory | ImplementationFlow | feature_addition |
| UI | UIFactory | UIFlow | feature_addition |
| ARCHITECTURE | ArchitectureFactory | ArchitectureFlow31 | documentation |
| API_CONTRACT | APIContractFactory | APIContractFlow | documentation |
| DOCUMENT | DocumentFactory | DocumentFlow | documentation |
| SECURITY | SecurityFactory | SecurityFlow | no_contribution |
| TEST | TestFactory | TestFlow | no_contribution |
| QA | QAFactory | QAFlow | no_contribution |
| [] | none | none | unknown (knowledge_edge) |

---

## B32-0D-043-002 — factory selection with all gap ID families

**Factory selection is predicate-driven, not gap-ID-driven:**

Regardless of which gap ID is present (style-001, hack-001, spike-001, prototype-001, rough-cut-001, dashboard-001):
- Factory selection depends solely on Predicates list
- Gap ID affects GapMetadata content only

---

## B32-0D-043-003 — GapMetadata structure: complete reference

**GapMetadata fields:**
```json
{
  "gapWarnings": ["<gap-id-1>", "..."],
  "capabilityGapWarning": "style_qualifier_noted",
  "domainId": "software-development"
}
```

Applied to: every flow created when GapWarnings is non-empty.
Not applied: when GapWarnings is empty (no gap vocabulary in request).
DomainId: present in flow metadata regardless of GapWarnings.

---

## B32-0D-043-004 — factory bypass conditions (complete)

**Factory is bypassed (knowledge_edge) when:**
- Predicates = [] (regardless of gap vocabulary count)
- Every action verb in the request is negated
- Only gap vocabulary and negations present; no positive action verbs

---
