# mental-debug-SKILL.md — Debug C# problems systematically

## When to load
Load when a test fails unexpectedly or a result is wrong.

## Four debug moves (apply in order)

### Move 1 — Pattern first
What shape does the failure have?
- Wrong count? → off-by-one or wrong split
- Zero accuracy? → all predictions one class (data collapse)
- Negative delta? → baseline too strong or wrong metric direction

### Move 2 — Contextual gap
List what the failing code NEEDS vs what it RECEIVES.
Find the first line where NEEDS ≠ RECEIVES.

### Move 3 — Mental execution
Run the function in your head with the smallest failing input.
Check every branch. Check every index.

### Move 4 — Data lifecycle
Trace the variable from creation to assertion.
Find the first step where the assumption breaks.
