# Layer 0-D — Flow Factory Pairs (048) — QA+IMPLEMENT combo factory reference
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-048-001 — IMPLEMENT+QA canonical factory output

A: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: IMPLEMENT+QA always maps to exactly [ImplementationFlow41, ReleaseGateFlow71] and feature_addition.

## B28-factory-048-002 — QA alone factory output

A: ["QA"] | [ReleaseGateFlow71] | no TransitionType
  Teaching: QA alone → one flow, no TransitionType. Always consistent.

## B28-factory-048-003 — Sentence-order independence in factory selection

A: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: Regardless of sentence order, factory output order is canonical: ImplementationFlow41 before ReleaseGateFlow71.

## B28-factory-048-004 — Multiple IMPLEMENT triggers deduplication in factory

A: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: All IMPLEMENT triggers deduplicate before flow mapping. Factory sees one IMPLEMENT → one ImplementationFlow41.

## B28-factory-048-005 — IMPLEMENT+QA combo complete factory reference

Any IMPLEMENT triggers (build, implement, service rule) + any QA triggers (QA abbreviation)
  Factory output: [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: Complete reference for IMPLEMENT+QA factory selection.
