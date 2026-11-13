# Why A Small Local Model Can Learn Legacy Code Documentation
# Version: 1.0 - 2026-05-11

---

## Claim

A small local model can become useful for legacy .NET documentation when the
task is decomposed into a routed, evidence-driven curriculum.

It does not need to understand every system globally in one prompt.

It needs to learn:

- how to document one method accurately
- how to preserve side effects and error paths
- how to use accepted callee summaries
- how to climb the call tree
- how to avoid invented architecture

---

## Why This Matches Our Existing Experiments

The orchestration and translation work showed the same pattern:

- broad mixed batches are noisy
- small measured 50-pair increments are easier to diagnose
- routing improves when it is trained as a flow, not hand-coded as a switch
- negative DPO examples matter most when they are close and specific
- promotion should depend on measured held-out behavior, not hope

Legacy code documentation has the same shape. The model does not need to
generate new architecture. It must translate code evidence into documentation
and propagate known facts upward.

---

## Why Bottom-Up Works

Legacy systems are often hard because high-level intent is not written down.
But the code still contains local facts:

- a method reads session state
- a repository writes a table
- a controller calls a service
- a WebForms event handler updates a grid
- an HttpModule intercepts a request

Once these facts are documented at the lowest level, callers can be documented
with evidence instead of guesswork.

This is compiler-like:

```text
method facts -> caller facts -> class facts -> workflow facts
```

The small model learns each stage separately.

---

## Limits

The model is not automatically trusted for:

- legal/regulatory documentation
- security-critical behavior
- undocumented business policy
- production refactoring decisions
- code it cannot parse or inspect

Those require human review or a stronger arbitration step.

