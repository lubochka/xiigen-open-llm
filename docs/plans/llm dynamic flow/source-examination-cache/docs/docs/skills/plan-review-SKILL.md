# plan-review-SKILL.md — Check a plan before implementation

## When to load
Load for ARCHITECT sessions before writing a plan.
Load for REVIEW sessions when evaluating a submitted plan.

## Five questions before writing code

1. Does every placement have a Gate B? (measurable pass condition)
2. Does every placement name exactly which file changes?
3. Are new tests listed by name and expected assertion?
4. Is the commit message specified?
5. Is the test count baseline stated?

If any answer is "no": add the missing element before starting.

## No product decisions rule

If a plan requires choosing between two design approaches
that affect user behaviour or architecture:
STOP — present both options to Luba, wait for decision.
Do not self-select between design alternatives.
