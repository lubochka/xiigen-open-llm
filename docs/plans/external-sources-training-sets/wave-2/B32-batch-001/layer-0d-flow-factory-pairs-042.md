# Layer 0-D — Flow Factory Pairs (042)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-042-001 — DomainId invariant in factory output

**Rule: all flows carry DomainId = "software-development"**

```
For any factory F invoked:
  F.Flow.DomainId = "software-development"
  F.Flow.GapMetadata.domainId = "software-development"
```

This applies to ImplementationFlow, UIFlow, ArchitectureFlow31, APIContractFlow, SecurityFlow, TestFlow, DocumentFlow, QAFlow.

---

## B32-0D-042-002 — multi-gap factory metadata

**Request:** "Expedient spike — implement the dashboard."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-spike-001, gap-vibe-coding-dashboard-001]

Factory selected: ImplementationFactory
Flow created: ImplementationFlow
GapMetadata: {
  gapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-spike-001, gap-vibe-coding-dashboard-001],
  capabilityGapWarning: style_qualifier_noted,
  domainId: "software-development"
}
```

---

## B32-0D-042-003 — factory metadata: no gap vocabulary case

**Request:** "Build the service."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: []

Factory selected: ImplementationFactory
Flow created: ImplementationFlow
GapMetadata: {} (empty; no gap vocabulary)
DomainId: "software-development"
```

DomainId still present even when GapMetadata is empty.

---

## B32-0D-042-004 — all gap IDs to factory metadata: reference

**Factory receives GapMetadata with all applicable gap IDs:**

GapMetadata.gapWarnings contains one entry per distinct gap ID triggered in the request.
GapMetadata.capabilityGapWarning = "style_qualifier_noted" (constant for all B32 gap scenarios).
GapMetadata.domainId = "software-development" (invariant).

---
