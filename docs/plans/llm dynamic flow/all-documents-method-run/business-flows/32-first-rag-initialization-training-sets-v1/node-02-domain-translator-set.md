# Node-02 - Domain Translator Set

## Purpose

Runtime A is one prompt fragment paired with one domain label.
Runtime B is the same request written in that domain's precise working language.

## Pair 2.1 - source and skill inventory

A: Prompt fragment: "Launch retrieval on day one using existing skills, task types, and source documents." Domain label: source and skill inventory.

B: The source and skill inventory responsibility must be represented as a separate territory in the first retrieval initialization flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Launch retrieval on day one using existing skills, task types, and source documents.

Pair 2.1 - Rephrased

A: Domain label: source and skill inventory. Fragment: "Begin retrieval on day one using existing skills, task types, and source documents."

B: Treat source and skill inventory as its own responsibility inside first retrieval initialization. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.2 - metadata schema

A: Prompt fragment: "Define metadata so each source can be filtered by skill, task type, version, audience, and trust level." Domain label: metadata schema.

B: The metadata schema responsibility must be represented as a separate territory in the first retrieval initialization flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Define metadata so each source can be filtered by skill, task type, version, audience, and trust level.

Pair 2.2 - Rephrased

A: Domain label: metadata schema. Fragment: "Set metadata so each source can be filtered by skill, task type, version, audience, and trust level."

B: Treat metadata schema as its own responsibility inside first retrieval initialization. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.3 - priority scoring

A: Prompt fragment: "Prioritize which skill and task mappings matter first by impact, frequency, confidence, cost, and latency risk." Domain label: priority scoring.

B: The priority scoring responsibility must be represented as a separate territory in the first retrieval initialization flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Prioritize which skill and task mappings matter first by impact, frequency, confidence, cost, and latency risk.

Pair 2.3 - Rephrased

A: Domain label: priority scoring. Fragment: "Order which skill and task mappings matter first by impact, frequency, confidence, cost, and latency risk."

B: Treat priority scoring as its own responsibility inside first retrieval initialization. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.4 - ingestion and validation

A: Prompt fragment: "Ingest documents through validation, duplicate handling, chunking, enrichment, and embedding." Domain label: ingestion and validation.

B: The ingestion and validation responsibility must be represented as a separate territory in the first retrieval initialization flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Ingest documents through validation, duplicate handling, chunking, enrichment, and embedding.

Pair 2.4 - Rephrased

A: Domain label: ingestion and validation. Fragment: "Bring in documents through validation, duplicate handling, chunking, enrichment, and embedding."

B: Treat ingestion and validation as its own responsibility inside first retrieval initialization. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.5 - index and storage choice

A: Prompt fragment: "Choose index shape and storage so lexical and semantic search can cooperate." Domain label: index and storage choice.

B: The index and storage choice responsibility must be represented as a separate territory in the first retrieval initialization flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Choose index shape and storage so lexical and semantic search can cooperate.

Pair 2.5 - Rephrased

A: Domain label: index and storage choice. Fragment: "Select index shape and storage so lexical and semantic search can cooperate."

B: Treat index and storage choice as its own responsibility inside first retrieval initialization. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.6 - hybrid retrieval

A: Prompt fragment: "Retrieve with hybrid recall, then rerank candidates for better precision." Domain label: hybrid retrieval.

B: The hybrid retrieval responsibility must be represented as a separate territory in the first retrieval initialization flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Retrieve with hybrid recall, then rerank candidates for better precision.

Pair 2.6 - Rephrased

A: Domain label: hybrid retrieval. Fragment: "Find with hybrid recall, then rerank candidates for better precision."

B: Treat hybrid retrieval as its own responsibility inside first retrieval initialization. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.7 - context management

A: Prompt fragment: "Manage context so the final answer gets enough evidence without overloading the model." Domain label: context management.

B: The context management responsibility must be represented as a separate territory in the first retrieval initialization flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Manage context so the final answer gets enough evidence without overloading the model.

Pair 2.7 - Rephrased

A: Domain label: context management. Fragment: "Handle context so the final answer gets enough evidence without overloading the model."

B: Treat context management as its own responsibility inside first retrieval initialization. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.8 - cold start evaluation

A: Prompt fragment: "Create synthetic launch-day questions to test retrieval before real user logs exist." Domain label: cold start evaluation.

B: The cold start evaluation responsibility must be represented as a separate territory in the first retrieval initialization flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Create synthetic launch-day questions to test retrieval before real user logs exist.

Pair 2.8 - Rephrased

A: Domain label: cold start evaluation. Fragment: "Help me create synthetic launch-day questions to test retrieval before real user logs exist."

B: Treat cold start evaluation as its own responsibility inside first retrieval initialization. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.9 - monitoring and rollout

A: Prompt fragment: "Monitor retrieval quality, latency, coverage, failures, and source freshness." Domain label: monitoring and rollout.

B: The monitoring and rollout responsibility must be represented as a separate territory in the first retrieval initialization flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Monitor retrieval quality, latency, coverage, failures, and source freshness.

Pair 2.9 - Rephrased

A: Domain label: monitoring and rollout. Fragment: "Watch retrieval quality, latency, coverage, failures, and source freshness."

B: Treat monitoring and rollout as its own responsibility inside first retrieval initialization. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.10 - monitoring and rollout

A: Prompt fragment: "Roll out retrieval safely with fallback templates and confidence thresholds." Domain label: monitoring and rollout.

B: The monitoring and rollout responsibility must be represented as a separate territory in the first retrieval initialization flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Roll out retrieval safely with fallback templates and confidence thresholds.

Pair 2.10 - Rephrased

A: Domain label: monitoring and rollout. Fragment: "Roll out retrieval safely with fallback templates and confidence thresholds."

B: Treat monitoring and rollout as its own responsibility inside first retrieval initialization. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

