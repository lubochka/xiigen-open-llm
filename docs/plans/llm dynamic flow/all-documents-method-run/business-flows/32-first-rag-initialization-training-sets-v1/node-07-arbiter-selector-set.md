# Node-07 - Arbiter Selector Set

## Purpose

Runtime A is the formal result collection before formatting.
Runtime B is the list of review units that need to fire.

## Pair 7.1 - ordering reviewer

A: Formal results include source and skill inventory, metadata schema, task type to source mapping for first retrieval initialization. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [ordering reviewer, consistency reviewer]. Reason: the result collection contains source and skill inventory, metadata schema, task type to source mapping and must be checked before any approved translation reaches the formatter.

Pair 7.1 - Rephrased

A: R1...Rn for first retrieval initialization includes branches about source and skill inventory, metadata schema, task type to source mapping.

B: Required reviewers: ordering reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.2 - privacy reviewer

A: Formal results include metadata schema, task type to source mapping, priority scoring for first retrieval initialization. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [privacy reviewer, consistency reviewer]. Reason: the result collection contains metadata schema, task type to source mapping, priority scoring and must be checked before any approved translation reaches the formatter.

Pair 7.2 - Rephrased

A: R1...Rn for first retrieval initialization includes branches about metadata schema, task type to source mapping, priority scoring.

B: Required reviewers: privacy reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.3 - business-rule reviewer

A: Formal results include task type to source mapping, priority scoring, ingestion and validation for first retrieval initialization. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [business-rule reviewer, consistency reviewer]. Reason: the result collection contains task type to source mapping, priority scoring, ingestion and validation and must be checked before any approved translation reaches the formatter.

Pair 7.3 - Rephrased

A: R1...Rn for first retrieval initialization includes branches about task type to source mapping, priority scoring, ingestion and validation.

B: Required reviewers: business-rule reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.4 - safety reviewer

A: Formal results include priority scoring, ingestion and validation, deduplication and chunking for first retrieval initialization. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [safety reviewer, consistency reviewer]. Reason: the result collection contains priority scoring, ingestion and validation, deduplication and chunking and must be checked before any approved translation reaches the formatter.

Pair 7.4 - Rephrased

A: R1...Rn for first retrieval initialization includes branches about priority scoring, ingestion and validation, deduplication and chunking.

B: Required reviewers: safety reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.5 - consistency reviewer

A: Formal results include ingestion and validation, deduplication and chunking, enrichment and embeddings for first retrieval initialization. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [consistency reviewer, consistency reviewer]. Reason: the result collection contains ingestion and validation, deduplication and chunking, enrichment and embeddings and must be checked before any approved translation reaches the formatter.

Pair 7.5 - Rephrased

A: R1...Rn for first retrieval initialization includes branches about ingestion and validation, deduplication and chunking, enrichment and embeddings.

B: Required reviewers: consistency reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.6 - measurement reviewer

A: Formal results include deduplication and chunking, enrichment and embeddings, index and storage choice for first retrieval initialization. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [measurement reviewer, consistency reviewer]. Reason: the result collection contains deduplication and chunking, enrichment and embeddings, index and storage choice and must be checked before any approved translation reaches the formatter.

Pair 7.6 - Rephrased

A: R1...Rn for first retrieval initialization includes branches about deduplication and chunking, enrichment and embeddings, index and storage choice.

B: Required reviewers: measurement reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.7 - recovery reviewer

A: Formal results include enrichment and embeddings, index and storage choice, hybrid retrieval for first retrieval initialization. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [recovery reviewer, consistency reviewer]. Reason: the result collection contains enrichment and embeddings, index and storage choice, hybrid retrieval and must be checked before any approved translation reaches the formatter.

Pair 7.7 - Rephrased

A: R1...Rn for first retrieval initialization includes branches about enrichment and embeddings, index and storage choice, hybrid retrieval.

B: Required reviewers: recovery reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.8 - audience-clarity reviewer

A: Formal results include index and storage choice, hybrid retrieval, reranking and relevance for first retrieval initialization. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [audience-clarity reviewer, consistency reviewer]. Reason: the result collection contains index and storage choice, hybrid retrieval, reranking and relevance and must be checked before any approved translation reaches the formatter.

Pair 7.8 - Rephrased

A: R1...Rn for first retrieval initialization includes branches about index and storage choice, hybrid retrieval, reranking and relevance.

B: Required reviewers: audience-clarity reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

