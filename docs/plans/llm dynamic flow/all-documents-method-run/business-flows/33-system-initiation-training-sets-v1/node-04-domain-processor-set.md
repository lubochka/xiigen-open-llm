# Node-04 - Domain Processor Set

## Purpose

Runtime A is one planned work piece.
Runtime B is the complete processed result for that piece, including success, waiting, and failure states.

## Pair 4.1 - kernel boot

A: Work piece: handle kernel boot for this request: "Start with a tiny stable kernel and let the engine generate the rest from plans."

B: Processed result: kernel boot has the necessary context, expected outcome, dependencies, and visible states. Success means the system initiation request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.1 - Rephrased

A: Domain work item: kernel boot. Runtime request: "Begin with a tiny stable kernel and let the engine generate the rest from plans."

B: The kernel boot result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.2 - fabric primitives

A: Work piece: handle fabric primitives for this request: "Import approved plans into registries so the runtime can understand families, tasks, flows, and rules."

B: Processed result: fabric primitives has the necessary context, expected outcome, dependencies, and visible states. Success means the system initiation request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.2 - Rephrased

A: Domain work item: fabric primitives. Runtime request: "Bring in approved plans into registries so the runtime can understand families, tasks, flows, and rules."

B: The fabric primitives result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.3 - flow runtime

A: Work piece: handle flow runtime for this request: "Provision a build workspace for generated work and tests."

B: Processed result: flow runtime has the necessary context, expected outcome, dependencies, and visible states. Success means the system initiation request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.3 - Rephrased

A: Domain work item: flow runtime. Runtime request: "Prepare a build workspace for generated work and tests."

B: The flow runtime result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.4 - plan import registries

A: Work piece: handle plan import registries for this request: "Run factory families side by side instead of one long manual sequence."

B: Processed result: plan import registries has the necessary context, expected outcome, dependencies, and visible states. Success means the system initiation request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.4 - Rephrased

A: Domain work item: plan import registries. Runtime request: "Set up factory families side by side instead of one long manual sequence."

B: The plan import registries result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.5 - build workspace

A: Work piece: handle build workspace for this request: "Generate server-side family work with contracts, behavior, tests, and review gates."

B: Processed result: build workspace has the necessary context, expected outcome, dependencies, and visible states. Success means the system initiation request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.5 - Rephrased

A: Domain work item: build workspace. Runtime request: "Produce server-side family work with contracts, behavior, tests, and review gates."

B: The build workspace result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.6 - parallel factory branches

A: Work piece: handle parallel factory branches for this request: "Generate client-side screens or flows with the selected client target and visual checks."

B: Processed result: parallel factory branches has the necessary context, expected outcome, dependencies, and visible states. Success means the system initiation request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.6 - Rephrased

A: Domain work item: parallel factory branches. Runtime request: "Produce client-side screens or flows with the selected client target and visual checks."

B: The parallel factory branches result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.7 - server-side family work

A: Work piece: handle server-side family work for this request: "Join all branches and promote only after tests, review, and evidence pass."

B: Processed result: server-side family work has the necessary context, expected outcome, dependencies, and visible states. Success means the system initiation request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.7 - Rephrased

A: Domain work item: server-side family work. Runtime request: "Combine all branches and promote only after tests, review, and evidence pass."

B: The server-side family work result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.8 - client-side family work

A: Work piece: handle client-side family work for this request: "Use retrieval maps so each build node receives the right source context."

B: Processed result: client-side family work has the necessary context, expected outcome, dependencies, and visible states. Success means the system initiation request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.8 - Rephrased

A: Domain work item: client-side family work. Runtime request: "Use retrieval maps so each build node receives the right source context."

B: The client-side family work result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

