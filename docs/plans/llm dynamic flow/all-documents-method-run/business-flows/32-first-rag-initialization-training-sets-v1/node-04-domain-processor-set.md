# Node-04 - Domain Processor Set

## Purpose

Runtime A is one planned work piece.
Runtime B is the complete processed result for that piece, including success, waiting, and failure states.

## Pair 4.1 - source and skill inventory

A: Work piece: handle source and skill inventory for this request: "Launch retrieval on day one using existing skills, task types, and source documents."

B: Processed result: source and skill inventory has the necessary context, expected outcome, dependencies, and visible states. Success means the first retrieval initialization request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.1 - Rephrased

A: Domain work item: source and skill inventory. Runtime request: "Begin retrieval on day one using existing skills, task types, and source documents."

B: The source and skill inventory result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.2 - metadata schema

A: Work piece: handle metadata schema for this request: "Define metadata so each source can be filtered by skill, task type, version, audience, and trust level."

B: Processed result: metadata schema has the necessary context, expected outcome, dependencies, and visible states. Success means the first retrieval initialization request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.2 - Rephrased

A: Domain work item: metadata schema. Runtime request: "Set metadata so each source can be filtered by skill, task type, version, audience, and trust level."

B: The metadata schema result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.3 - task type to source mapping

A: Work piece: handle task type to source mapping for this request: "Prioritize which skill and task mappings matter first by impact, frequency, confidence, cost, and latency risk."

B: Processed result: task type to source mapping has the necessary context, expected outcome, dependencies, and visible states. Success means the first retrieval initialization request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.3 - Rephrased

A: Domain work item: task type to source mapping. Runtime request: "Order which skill and task mappings matter first by impact, frequency, confidence, cost, and latency risk."

B: The task type to source mapping result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.4 - priority scoring

A: Work piece: handle priority scoring for this request: "Ingest documents through validation, duplicate handling, chunking, enrichment, and embedding."

B: Processed result: priority scoring has the necessary context, expected outcome, dependencies, and visible states. Success means the first retrieval initialization request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.4 - Rephrased

A: Domain work item: priority scoring. Runtime request: "Bring in documents through validation, duplicate handling, chunking, enrichment, and embedding."

B: The priority scoring result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.5 - ingestion and validation

A: Work piece: handle ingestion and validation for this request: "Choose index shape and storage so lexical and semantic search can cooperate."

B: Processed result: ingestion and validation has the necessary context, expected outcome, dependencies, and visible states. Success means the first retrieval initialization request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.5 - Rephrased

A: Domain work item: ingestion and validation. Runtime request: "Select index shape and storage so lexical and semantic search can cooperate."

B: The ingestion and validation result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.6 - deduplication and chunking

A: Work piece: handle deduplication and chunking for this request: "Retrieve with hybrid recall, then rerank candidates for better precision."

B: Processed result: deduplication and chunking has the necessary context, expected outcome, dependencies, and visible states. Success means the first retrieval initialization request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.6 - Rephrased

A: Domain work item: deduplication and chunking. Runtime request: "Find with hybrid recall, then rerank candidates for better precision."

B: The deduplication and chunking result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.7 - enrichment and embeddings

A: Work piece: handle enrichment and embeddings for this request: "Manage context so the final answer gets enough evidence without overloading the model."

B: Processed result: enrichment and embeddings has the necessary context, expected outcome, dependencies, and visible states. Success means the first retrieval initialization request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.7 - Rephrased

A: Domain work item: enrichment and embeddings. Runtime request: "Handle context so the final answer gets enough evidence without overloading the model."

B: The enrichment and embeddings result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

## Pair 4.8 - index and storage choice

A: Work piece: handle index and storage choice for this request: "Create synthetic launch-day questions to test retrieval before real user logs exist."

B: Processed result: index and storage choice has the necessary context, expected outcome, dependencies, and visible states. Success means the first retrieval initialization request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.8 - Rephrased

A: Domain work item: index and storage choice. Runtime request: "Help me create synthetic launch-day questions to test retrieval before real user logs exist."

B: The index and storage choice result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

