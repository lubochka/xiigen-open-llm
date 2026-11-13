# Layer 2 — Nine-Node Flow Pairs (024) — Complete nine-node rule summary for sub-phase 5.1
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## N024 — Nine-node flow rule summary for UnitTestingFlow51 (sub-phase 5.1)

### Node names and responsibilities

| Node | Name | Responsibility |
|------|------|----------------|
| 01 | DomainSplitterUnit | Splits the service into test domain buckets (e.g., calculation, validation, edge states) |
| 02 | DomainTranslatorUnit | Translates each domain bucket into testable method boundaries |
| 03 | SchemePlannerUnit | Produces the test suite schema: class names, method names, framework, pattern (AAA) |
| 04 | DomainProcessorUnit | Identifies dependencies and selects test doubles (mocks, stubs, fakes) |
| 05 | ApplierUnit | Produces the actual test code / test count |
| 06 | ResultTranslatorUnit | Verifies that produced tests pass |
| 07 | ArbiterSelectorUnit | Runs the quality gate: coverage estimate, warnings, rejection if applicable |
| 08 | ArbiterUnit | Issues the final Verdict: PASS / PASS_WITH_WARNINGS / REJECTED |
| 09 | FormatterUnit | Formats the output summary and file list |

Node 01 always carries the label: SelectedLogicalTransition

### Flow selection rules

| Predicates | FlowId | SubPhase |
|------------|--------|----------|
| TEST | UnitTestingFlow51 | 5.1 |
| TEST+IMPLEMENT | UnitTestingFlow51 + BackendServiceFlow41 | 5.1 + 4.1 |
| TEST+SECURITY | UnitTestingFlow51 + SecurityComplianceFlow52 | 5.1 + 5.2 |
| [] (run/execute) | No flow created | null → knowledge_edge |

### Quality gate outcomes

PASS: all tests pass, coverage ≥ 85%
PASS_WITH_WARNINGS: tests pass but coverage < 85% or critical path not covered
REJECTED: mixed-predicate routing conflict; incomplete domain; zero tests produced

### TransitionType rules (summary)

TEST alone → documentation
TEST + any IMPLEMENT verb → feature_addition
TEST + refactoring verb → refactoring
[] → unknown
