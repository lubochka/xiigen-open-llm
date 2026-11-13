# Phase Registry and Coverage Matrix
# Date: 2026-05-04

## Phase Registry

| Phase | Name | Status | Meaning |
|---:|---|---|---|
| 1 | Design | Ready | All five design sub-phases have candidate source documents. |
| 2 | Functional Specification | Ready | All four functional sub-phases have candidate source documents. |
| 3 | Technical Design | Ready | All five technical-design sub-phases have strong source coverage. |
| 4 | Implementation | Ready | All five implementation sub-phases have strong source coverage. |
| 5 | Testing | Ready with one boundary | Unit, E2E, and performance coverage exist; integration-test authority is missing. |
| 6 | QA | Ready with one gap and one thin boundary | Functional and regression review exist; security/compliance is missing; accessibility audit is thin. |
| 7 | QA Automation | Partial boundary | CI/CD and reporting are strong; scenario automation and regression-suite maintenance are missing. |

## Sub-Phase Coverage

| Sub-phase | Name | Status |
|---|---|---|
| 1.1 | Design System and Tokens | covered_for_experiment |
| 1.2 | Screen and Layout Structure | covered_for_experiment |
| 1.3 | Interaction Model and State Transitions | covered_for_experiment |
| 1.4 | Accessibility and Compliance | covered_for_experiment |
| 1.5 | Motion and Animation | covered_for_experiment |
| 2.1 | User Stories and Acceptance Criteria | covered_for_experiment |
| 2.2 | Business Rules | covered_for_experiment |
| 2.3 | Integration Point Specification | covered_for_experiment |
| 2.4 | Data Model Boundaries | covered_for_experiment |
| 3.1 | Architecture and Component Topology | covered_for_experiment |
| 3.2 | Data Schema and Migration Design | covered_for_experiment |
| 3.3 | API Contract Definition | covered_for_experiment |
| 3.4 | Security and Authorization Model | covered_for_experiment |
| 3.5 | Infrastructure and Deployment Topology | covered_for_experiment |
| 4.1 | Backend Services | covered_for_experiment |
| 4.2 | Frontend and Client Rendering | covered_for_experiment |
| 4.3 | Integration and Provider Connectors | covered_for_experiment |
| 4.4 | Data Migration Execution | covered_for_experiment |
| 4.5 | Documentation | covered_for_experiment |
| 5.1 | Unit and Contract Tests | covered_for_experiment |
| 5.2 | Integration Tests | knowledge_edge_or_missing_source |
| 5.3 | End-to-End Scenario Tests | covered_for_experiment |
| 5.4 | Performance and Load Tests | covered_for_experiment |
| 6.1 | Functional Review | covered_for_experiment |
| 6.2 | Regression Coverage Verification | covered_for_experiment |
| 6.3 | Security and Compliance Review | knowledge_edge_or_missing_source |
| 6.4 | Accessibility Audit | thin_boundary |
| 7.1 | Test Scenario Automation | knowledge_edge_or_missing_source |
| 7.2 | Regression Suite Maintenance | knowledge_edge_or_missing_source |
| 7.3 | CI/CD Gate Configuration | covered_for_experiment |
| 7.4 | Coverage and Reporting | covered_for_experiment |

## What This Means For Learning Sets

Learning-set construction can start only where experiment source authority is
covered or intentionally thin. It should not start in missing-source sub-phases
until source documents are produced or selected.

Allowed now:

- Phase 3 technical-design transition experiments.
- Phase 4 implementation transition experiments.
- Phase 1 design and Phase 2 functional-spec experiments where a specific
  source document is manually selected from the registry.
- E53-derived learning-set conversion where the oracle already exists.

Not allowed yet:

- Broad Phase 5 integration-test learning sets.
- Broad Phase 6 security/compliance review learning sets.
- Broad Phase 7 QA automation learning sets.

Those areas need source authority first. Their absence should become a named
knowledge edge in any orchestration run that touches them.

