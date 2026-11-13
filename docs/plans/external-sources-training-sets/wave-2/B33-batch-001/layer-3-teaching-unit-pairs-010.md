# Layer 3 — Teaching Unit Pairs (010)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-010 — TransitionType determination in B33 context

**Teaching goal**: TransitionType rules apply identically in AI context — no AI-specific overrides.

**TransitionType hierarchy** (highest priority first):
1. "refactor" verb present → refactoring (always, regardless of other verbs)
2. IMPLEMENT present (no refactor) → feature_addition
3. TEST and/or DOCUMENT present, IMPLEMENT absent → documentation
4. QA alone → no contribution (QA never drives TransitionType)
5. Zero predicates (gap) → no TransitionType

**B33-specific examples**:

| Request | Predicates | TransitionType |
|---------|------------|----------------|
| "Build the agent skill router" | [IMPLEMENT] | feature_addition |
| "Refactor the agent skill router" | [IMPLEMENT] | refactoring |
| "Design the agent architecture" | [ARCHITECTURE] | documentation |
| "Write tests for the agent gateway" | [TEST] | documentation |
| "QA the agent release" | [QA] | no contribution |
| "Adapt the agent for finance" | [] gap | none |
| "Build and refactor the agent memory" | [IMPLEMENT] | refactoring |

**ARCHITECTURE alone → documentation** (even though it produces design artifacts, IMPLEMENT is absent).
**Note**: There is no "architecture_decision" TransitionType. ARCHITECTURE without IMPLEMENT → documentation.
