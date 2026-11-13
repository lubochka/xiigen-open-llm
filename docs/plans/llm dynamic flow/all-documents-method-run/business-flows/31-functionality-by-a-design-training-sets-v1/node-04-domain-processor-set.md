# Node-04 - Domain Processor Set

## Purpose

Runtime A is one planned work piece.
Runtime B is the complete processed result for that piece, including success, waiting, and failure states.

## Pair 4.1 - design structure extraction

A: Work piece: handle design structure extraction for this request: "Read all design screens and identify components, actions, entities, navigation, and functional meaning."

B: Processed result: design structure extraction has the necessary context, expected outcome, dependencies, and visible states. Success means the functionality by design request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.1 - Rephrased

A: Domain work item: design structure extraction. Runtime request: "Examine all design screens and identify components, actions, entities, navigation, and functional meaning."

B: The design structure extraction result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.2 - screen semantic meaning

A: Work piece: handle screen semantic meaning for this request: "Map a screen to known module signatures with evidence rather than guessing."

B: Processed result: screen semantic meaning has the necessary context, expected outcome, dependencies, and visible states. Success means the functionality by design request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.2 - Rephrased

A: Domain work item: screen semantic meaning. Runtime request: "Provide me a view of a screen to known module signatures with evidence rather than guessing."

B: The screen semantic meaning result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.3 - system model inference

A: Work piece: handle system model inference for this request: "Infer whether the design is a shop, social network, report, dashboard, or another system type."

B: Processed result: system model inference has the necessary context, expected outcome, dependencies, and visible states. Success means the functionality by design request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.3 - Rephrased

A: Domain work item: system model inference. Runtime request: "Infer whether the design is a shop, social network, report, dashboard, or another system type."

B: The system model inference result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.4 - module ontology

A: Work piece: handle module ontology for this request: "Use similar known screens and graph constraints together to decide what the design means."

B: Processed result: module ontology has the necessary context, expected outcome, dependencies, and visible states. Success means the functionality by design request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.4 - Rephrased

A: Domain work item: module ontology. Runtime request: "Use similar known screens and graph constraints together to decide what the design means."

B: The module ontology result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.5 - similarity retrieval

A: Work piece: handle similarity retrieval for this request: "Detect missing screens or functions from the inferred system type."

B: Processed result: similarity retrieval has the necessary context, expected outcome, dependencies, and visible states. Success means the functionality by design request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.5 - Rephrased

A: Domain work item: similarity retrieval. Runtime request: "Detect missing screens or functions from the inferred system type."

B: The similarity retrieval result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.6 - graph constraints

A: Work piece: handle graph constraints for this request: "Extract stable deterministic features such as frames, text, forms, buttons, lists, and navigation."

B: Processed result: graph constraints has the necessary context, expected outcome, dependencies, and visible states. Success means the functionality by design request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.6 - Rephrased

A: Domain work item: graph constraints. Runtime request: "Extract stable deterministic features such as frames, text, forms, buttons, lists, and navigation."

B: The graph constraints result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.7 - deterministic screen features

A: Work piece: handle deterministic screen features for this request: "Store screen examples so the engine learns typical components for future designs."

B: Processed result: deterministic screen features has the necessary context, expected outcome, dependencies, and visible states. Success means the functionality by design request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.7 - Rephrased

A: Domain work item: deterministic screen features. Runtime request: "Preserve screen examples so the engine learns typical components for future designs."

B: The deterministic screen features result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.8 - multimodal screen reading

A: Work piece: handle multimodal screen reading for this request: "Reject a system-type claim when it has no evidence attached."

B: Processed result: multimodal screen reading has the necessary context, expected outcome, dependencies, and visible states. Success means the functionality by design request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.8 - Rephrased

A: Domain work item: multimodal screen reading. Runtime request: "Refuse a system-type claim when it has no evidence attached."

B: The multimodal screen reading result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

