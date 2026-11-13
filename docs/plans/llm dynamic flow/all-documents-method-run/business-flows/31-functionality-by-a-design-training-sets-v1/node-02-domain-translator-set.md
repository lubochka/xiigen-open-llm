# Node-02 - Domain Translator Set

## Purpose

Runtime A is one prompt fragment paired with one domain label.
Runtime B is the same request written in that domain's precise working language.

## Pair 2.1 - design structure extraction

A: Prompt fragment: "Read all design screens and identify components, actions, entities, navigation, and functional meaning." Domain label: design structure extraction.

B: The design structure extraction responsibility must be represented as a separate territory in the functionality by design flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Read all design screens and identify components, actions, entities, navigation, and functional meaning.

Pair 2.1 - Rephrased

A: Domain label: design structure extraction. Fragment: "Examine all design screens and identify components, actions, entities, navigation, and functional meaning."

B: Treat design structure extraction as its own responsibility inside functionality by design. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.2 - screen to module mapping

A: Prompt fragment: "Map a screen to known module signatures with evidence rather than guessing." Domain label: screen to module mapping.

B: The screen to module mapping responsibility must be represented as a separate territory in the functionality by design flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Map a screen to known module signatures with evidence rather than guessing.

Pair 2.2 - Rephrased

A: Domain label: screen to module mapping. Fragment: "Provide me a view of a screen to known module signatures with evidence rather than guessing."

B: Treat screen to module mapping as its own responsibility inside functionality by design. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.3 - system type inference

A: Prompt fragment: "Infer whether the design is a shop, social network, report, dashboard, or another system type." Domain label: system type inference.

B: The system type inference responsibility must be represented as a separate territory in the functionality by design flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Infer whether the design is a shop, social network, report, dashboard, or another system type.

Pair 2.3 - Rephrased

A: Domain label: system type inference. Fragment: "Infer whether the design is a shop, social network, report, dashboard, or another system type."

B: Treat system type inference as its own responsibility inside functionality by design. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.4 - similarity retrieval

A: Prompt fragment: "Use similar known screens and graph constraints together to decide what the design means." Domain label: similarity retrieval.

B: The similarity retrieval responsibility must be represented as a separate territory in the functionality by design flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Use similar known screens and graph constraints together to decide what the design means.

Pair 2.4 - Rephrased

A: Domain label: similarity retrieval. Fragment: "Use similar known screens and graph constraints together to decide what the design means."

B: Treat similarity retrieval as its own responsibility inside functionality by design. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.5 - gap completion

A: Prompt fragment: "Detect missing screens or functions from the inferred system type." Domain label: gap completion.

B: The gap completion responsibility must be represented as a separate territory in the functionality by design flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Detect missing screens or functions from the inferred system type.

Pair 2.5 - Rephrased

A: Domain label: gap completion. Fragment: "Detect missing screens or functions from the inferred system type."

B: Treat gap completion as its own responsibility inside functionality by design. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.6 - deterministic screen features

A: Prompt fragment: "Extract stable deterministic features such as frames, text, forms, buttons, lists, and navigation." Domain label: deterministic screen features.

B: The deterministic screen features responsibility must be represented as a separate territory in the functionality by design flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Extract stable deterministic features such as frames, text, forms, buttons, lists, and navigation.

Pair 2.6 - Rephrased

A: Domain label: deterministic screen features. Fragment: "Extract stable deterministic features such as frames, text, forms, buttons, lists, and navigation."

B: Treat deterministic screen features as its own responsibility inside functionality by design. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.7 - feedback learning

A: Prompt fragment: "Store screen examples so the engine learns typical components for future designs." Domain label: feedback learning.

B: The feedback learning responsibility must be represented as a separate territory in the functionality by design flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Store screen examples so the engine learns typical components for future designs.

Pair 2.7 - Rephrased

A: Domain label: feedback learning. Fragment: "Preserve screen examples so the engine learns typical components for future designs."

B: Treat feedback learning as its own responsibility inside functionality by design. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.8 - evidence quality gates

A: Prompt fragment: "Reject a system-type claim when it has no evidence attached." Domain label: evidence quality gates.

B: The evidence quality gates responsibility must be represented as a separate territory in the functionality by design flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Reject a system-type claim when it has no evidence attached.

Pair 2.8 - Rephrased

A: Domain label: evidence quality gates. Fragment: "Refuse a system-type claim when it has no evidence attached."

B: Treat evidence quality gates as its own responsibility inside functionality by design. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.9 - dashboard and report recognition

A: Prompt fragment: "Recognize dashboard or report designs and map them to metrics, filters, and chart meaning." Domain label: dashboard and report recognition.

B: The dashboard and report recognition responsibility must be represented as a separate territory in the functionality by design flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Recognize dashboard or report designs and map them to metrics, filters, and chart meaning.

Pair 2.9 - Rephrased

A: Domain label: dashboard and report recognition. Fragment: "Recognize dashboard or report designs and map them to metrics, filters, and chart meaning."

B: Treat dashboard and report recognition as its own responsibility inside functionality by design. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.10 - design structure extraction

A: Prompt fragment: "Create a no-code flow where design input becomes module mapping and gap completion." Domain label: design structure extraction.

B: The design structure extraction responsibility must be represented as a separate territory in the functionality by design flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Create a no-code flow where design input becomes module mapping and gap completion.

Pair 2.10 - Rephrased

A: Domain label: design structure extraction. Fragment: "Help me create a no-code flow where design input becomes module mapping and gap completion."

B: Treat design structure extraction as its own responsibility inside functionality by design. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

