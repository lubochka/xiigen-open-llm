# LLM MVP Core — Plan for Implementing All 12 Sub-Projects
## Solution: LlmFromScratch.sln — Implementation Plan v2.0

**Project:** llm-mvp-core  
**Language:** C# / .NET 8  
**Constraint:** No external NuGet packages except TorchSharp for matrix operations  
**Document purpose:** Step-by-step implementation plan for each of the 12 sub-projects as independent class libraries within the same solution  
**This document is separate from all XIIGen work.**

**What changed in v2 vs v1:**
- Every sub-project now follows the nine coding rules (Genie DNA patterns) adapted for .NET
- Every sub-project is fabric-first: all external dependencies are hidden behind interfaces
- Every line of code in every example must carry a plain-language comment explaining what it does
- All abbreviations are written out in full with a plain-language explanation in parentheses

---

## Part 1 — The Nine Coding Rules (Genie DNA), adapted for .NET

These nine rules come from the XIIGen architecture. They are adapted here for C# but the intent is identical: every project in this solution must follow all nine rules. A project that violates even one rule is not complete.

### Coding rule 1 — No typed model classes (use dictionaries for data containers)

**What this means in .NET:** Instead of writing `class TokenEntry { public int Id; public string Text; }` and then a second class `class TokenResult { public TokenEntry[] Tokens; }` — use `Dictionary<string, object>` as the data container. The only exceptions are small, immutable value objects like `record AnalyzedToken` or `record GraphEdge` where the fields are fixed and never change.

**Why:** Typed model classes couple the code to a specific data shape. When the shape changes, the class changes, and everything using it breaks. Dictionaries decouple the data shape from the logic.

**How to check:** Search for `class` declarations. Every `class` must either extend the `BaseService` (coding rule 4) or be a pure-algorithm class (like `BM25Scorer`). Data containers must be `Dictionary<string, object>` or `record` with only primitive fields.

---

### Coding rule 2 — No hardcoded filter objects (use a filter builder method)

**What this means in .NET:** When searching the vector index or the graph, never write the search parameters inline inside the business method. Instead, call a dedicated `BuildSearchFilter(input)` method that assembles the filter object and returns it. The business method receives the result.

**Why:** Hardcoded filters are invisible to tests and cannot be changed without touching the business logic.

---

### Coding rule 3 — All methods return an operation result (never throw for business errors)

**What this means in .NET:** Define a generic wrapper:

```csharp
// OperationResult wraps any return value with a success flag and an error message.
// This means the caller always checks whether the operation worked
// instead of catching exceptions.
public record OperationResult<TValue>
{
    // Whether the operation completed without errors
    public bool Success { get; init; }

    // The actual returned value when the operation succeeded
    public TValue? Value { get; init; }

    // A plain-language description of what went wrong, or empty if nothing went wrong
    public string ErrorMessage { get; init; } = string.Empty;

    // A short code identifying the type of error (e.g. "NOT_FOUND", "INVALID_INPUT")
    public string ErrorCode { get; init; } = string.Empty;

    // Create a successful result that carries a value
    public static OperationResult<TValue> Ok(TValue value) =>
        new() { Success = true, Value = value };

    // Create a failed result that carries no value but explains what went wrong
    public static OperationResult<TValue> Fail(string message, string code = "") =>
        new() { Success = false, ErrorMessage = message, ErrorCode = code };
}
```

Every public method in every service must return `OperationResult<T>`. `throw` is forbidden for business errors. `throw` is allowed only for programming errors (null arguments, impossible states) that indicate a bug in the code itself.

---

### Coding rule 4 — All service classes extend the base service (inherit common behavior)

**What this means in .NET:** Every service class must extend `BaseService`, not implement an interface directly. `BaseService` provides:
- A logger (so every service logs in the same format)
- A scope context reader (so every service knows which tenant or session it is running in)
- A standard way to create `OperationResult` values

```csharp
// BaseService gives every service class a logger and a context reader for free.
// Any class that does real work must extend this — never instantiate a service
// without extending BaseService.
public abstract class BaseService
{
    // A logger that records what the service is doing, for debugging and monitoring
    protected readonly ILogger Logger;

    // A reader that tells the service which user or tenant context it is running in
    protected readonly IOperationContext OperationContext;

    protected BaseService(ILogger logger, IOperationContext context)
    {
        // Store the logger so any subclass can call Logger.LogInformation(...)
        Logger = logger;

        // Store the context reader so any subclass can call OperationContext.GetCurrentScope()
        OperationContext = context;
    }
}
```

---

### Coding rule 5 — Scope (tenant or session) comes from context, never from a parameter

**What this means in .NET:** No method may accept a `tenantId`, `sessionId`, or `scopeId` parameter. Instead, every service reads the current scope from `IOperationContext`. The context is set once at the entry point (the API controller or the background job runner) and flows through all service calls automatically.

**Why:** If scope is a parameter, callers can forget to pass it, or pass the wrong one. Context makes it impossible to forget.

---

### Coding rule 6 — No entity-specific API handlers (use a generic dispatcher)

**What this means in .NET:** No class like `TokenizerController` with routes `/tokenizer/encode` and `/tokenizer/decode`. Instead, one `OperationDispatcher` receives all requests and routes them to the right service based on the operation name. The dispatcher is registered once and handles everything.

---

### Coding rule 7 — Idempotency on every operation that can be retried (deduplicate by operation identifier)

**What this means in .NET:** Any method that writes data, trains weights, or sends a message must accept an `operationId` string. Before doing work, the method checks whether this `operationId` was already processed. If yes, it returns the previous result immediately without doing the work again.

```csharp
// Before doing any work, check whether we already processed this exact request.
// This prevents double-training or double-writing when a caller retries on network error.
public async Task<OperationResult<TrainingStepResult>> RunTrainingStepAsync(
    string operationId,
    TrainingBatch batch)
{
    // Ask the idempotency store whether this operationId was already processed
    var alreadyDone = await idempotencyStore.TryGetResultAsync(operationId);
    if (alreadyDone is not null)
    {
        // We already did this work — return the saved result without doing it again
        return OperationResult<TrainingStepResult>.Ok(alreadyDone);
    }

    // ... do the actual training step ...

    // Save the result so future calls with the same operationId get it back immediately
    await idempotencyStore.SaveResultAsync(operationId, result);
    return OperationResult<TrainingStepResult>.Ok(result);
}
```

---

### Coding rule 8 — Save data before dispatching a message (outbox pattern)

**What this means in .NET:** When a service both saves state and sends a notification, the save must always happen first. If the save fails, no notification is sent. If the notification fails after a successful save, the notification can be retried safely because the data is already there.

```csharp
// Step 1: save the checkpoint to disk first
await checkpointStorage.SaveAsync(checkpoint);

// Step 2: only after saving succeeds, send the "training step complete" notification
// If we reversed these two lines, the notification might arrive before the data
await eventDispatcher.DispatchAsync(new TrainingStepCompletedEvent(stepNumber));
```

---

### Coding rule 9 — Wrap all dispatched events in a standard envelope

**What this means in .NET:** All events dispatched between components must be wrapped in a standard envelope that carries the event type, a unique identifier (for deduplication per coding rule 7), the source component, and the payload.

```csharp
// StandardEvent is the envelope that every dispatched message must use.
// The envelope tells any receiver what kind of event this is, where it came from,
// and gives a unique identifier so duplicate deliveries can be ignored.
public record StandardEvent
{
    // A unique identifier for this specific event delivery (used for deduplication)
    public string EventId { get; init; } = Guid.NewGuid().ToString();

    // The type of event, written as a dot-separated path (e.g. "training.step.completed")
    public string EventType { get; init; } = string.Empty;

    // The name of the component that created this event
    public string Source { get; init; } = string.Empty;

    // The actual data payload, stored as a dictionary so it can hold any structure
    public Dictionary<string, object> Payload { get; init; } = new();
}
```

---

## Part 2 — Fabric-First Architecture for this Solution

Fabric-first means: every class that uses an external capability (file storage, model inference, vector search, graph storage) must talk to it through a C# interface, never through a concrete implementation directly. The concrete implementation is injected by the dependency injection container.

This makes it possible to swap the underlying technology without touching the business logic. For example, the vector search interface can be backed by the in-memory brute-force index during development and testing, and by the Hierarchical Navigable Small World (a hierarchical navigation graph — a data structure for fast approximate nearest-neighbor search) index in production.

### The five fabric interfaces for this solution

```csharp
// IVectorStorage: everything that stores and searches embedding vectors
// (numeric representations of text as multi-dimensional points in space)
// Any class that needs to store or find vectors must use this interface.
// Concrete implementations: InMemoryVectorStorage, HnswVectorStorage
public interface IVectorStorage
{
    // Store a vector (a numeric vector — a list of floating-point numbers)
    // together with a text identifier so it can be retrieved later
    Task<OperationResult<string>> AddAsync(float[] vector, string identifier);

    // Find the most similar vectors to the query vector and return their identifiers
    // topK means "return at most this many results"
    Task<OperationResult<List<VectorMatch>>> SearchAsync(float[] queryVector, int topK);
}

// IGraphStorage: everything that stores and traverses the knowledge graph
// (a knowledge graph — a data structure where nodes are code entities and edges are the relationships between them)
// Any class that needs to read or write graph data must use this interface.
// Concrete implementations: InMemoryGraphStorage, JsonFileGraphStorage
public interface IGraphStorage
{
    // Add a node (a node — one entity: a class, method, or file)
    Task<OperationResult<string>> AddNodeAsync(Dictionary<string, object> nodeData);

    // Add a directed edge (an edge — a relationship between two nodes, for example "calls" or "inherits")
    Task<OperationResult<string>> AddEdgeAsync(string fromNodeId, string toNodeId, string relationshipType, double weight);

    // Return all nodes reachable from a starting node within a given number of steps
    Task<OperationResult<List<Dictionary<string, object>>>> GetNeighboursAsync(string nodeId, int hops);
}

// IModelProvider: everything that runs a neural network forward pass
// (running data through a neural network to get a prediction)
// Any class that needs to run the model must use this interface.
// Concrete implementations: TorchSharpModelProvider, MockModelProvider
public interface IModelProvider
{
    // Run the model on a batch of token sequences and return the output logits
    // (logits — unnormalized probability scores for each token)
    Task<OperationResult<float[,,]>> ForwardAsync(long[,] tokenIds);

    // Load model weights from a file path into memory
    Task<OperationResult<bool>> LoadWeightsAsync(string checkpointPath);

    // Save the current model weights to a file path
    Task<OperationResult<bool>> SaveWeightsAsync(string checkpointPath);
}

// ITokenizerProvider: everything that converts text to numbers and back
// (tokenization — turning text into a list of numeric identifiers)
// Any class that needs to tokenize or decode must use this interface.
// Concrete implementations: BpeTokenizerProvider, MockTokenizerProvider
public interface ITokenizerProvider
{
    // Convert a string of text into a list of integer token identifiers
    OperationResult<List<int>> Encode(string text);

    // Convert a list of integer token identifiers back into a string of text
    OperationResult<string> Decode(List<int> tokenIds);

    // Load a previously trained vocabulary from disk
    OperationResult<bool> LoadVocabulary(string path);
}

// IIndexStorage: everything that stores and searches a full-text inverted index
// (an inverted index — a data structure for full-text search)
// Any class that needs to do keyword search must use this interface.
// Concrete implementations: InMemoryIndexStorage, DiskIndexStorage
public interface IIndexStorage
{
    // Add a document (its full text and a file path identifier) to the index
    OperationResult<int> AddDocument(string text, string filePath);

    // Search for documents matching a text query, return top results with scores
    OperationResult<List<TextSearchMatch>> Search(string query, int topK);

    // Save the entire index to a directory on disk
    Task<OperationResult<bool>> SaveAsync(string directory);

    // Load a previously saved index from a directory on disk
    Task<OperationResult<bool>> LoadAsync(string directory);
}
```

Every service in every sub-project receives its fabric dependencies through constructor injection. No service may `new` a concrete implementation directly.

---

## Part 3 — Comment Standard (every line must be explained)

Every line of code — including closing braces, variable declarations, and `return` statements — must carry a comment that explains, in the operator's language (plain language; Russian for the original author), what that line does and why. The comment must be written as if explaining to someone who has never written code before.

**Example of a correctly commented method:**

```csharp
// This method teaches the tokenizer which pairs of bytes appear together most often.
// It repeats the merging process until the vocabulary reaches the requested size.
public void Train(string text, int requestedVocabularySize)
{
    // Convert the input text into a list of individual byte values (numbers from 0 to 255)
    // Each byte becomes its own token at the start — we will merge them in later steps
    var tokens = Encoding.UTF8.GetBytes(text)
                              .Select(b => (int)b)
                              .ToList();

    // Keep merging pairs until we have as many tokens in the vocabulary as requested
    while (vocabulary.Count < requestedVocabularySize)
    {
        // Count how many times each adjacent pair of tokens appears next to each other
        // For example if the text contains "ab" three times, the pair (a, b) has count 3
        var pairCounts = CountAdjacentPairs(tokens);

        // If there are no pairs left to merge, we cannot grow the vocabulary further
        // This happens when every remaining token is a single unique byte
        if (pairCounts.Count == 0)
        {
            // Leave the loop early — nothing more can be merged
            break;
        }

        // Find the pair that appears most often
        // This pair will become a single new token
        var mostFrequentPair = pairCounts.MaxBy(pair => pair.Value).Key;

        // Assign a new integer identifier to this merged pair
        // The identifier continues from where the last one left off
        int newTokenId = vocabulary.Count;

        // Store the merge rule: when we see this pair in future text, use the new token id
        mergeTable[mostFrequentPair] = newTokenId;

        // Add the new token to the vocabulary so we can decode it later
        vocabulary[newTokenId] = CombineBytes(mostFrequentPair);

        // Replace every occurrence of this pair in the token list with the new single token
        tokens = ApplyMerge(tokens, mostFrequentPair, newTokenId);
    }
}
```

Every file must follow this style without exception.

---

## Part 4 — Abbreviation Rules

No abbreviation may appear in code, comments, documentation, or variable names without being written out in full with a plain-language explanation.

The table below lists all abbreviations used in this solution and their full forms:

| Short form | Full form | Plain-language explanation |
|---|---|---|
| Byte Pair Encoding (BPE) | Byte Pair Encoding | an algorithm that joins frequently occurring pairs of bytes into single tokens |
| Hierarchical Navigable Small World (HNSW) | Hierarchical Navigable Small World | a multi-layer graph structure for finding approximately nearest neighbors very quickly |
| Rotary Position Embedding (RoPE) | Rotary Position Embedding | a method that encodes token position by rotating the query and key vectors by an angle proportional to position |
| Grouped Query Attention (GQA) | Grouped Query Attention | an attention variant where several query heads share one key-value head to save memory |
| Root Mean Square normalization (RMS normalization) | Root Mean Square normalization | divides each vector by the square root of the average of its squared values, keeping numbers in a stable range |
| Direct Preference Optimization (DPO) | Direct Preference Optimization | a training method that teaches the model to prefer "good" responses over "bad" ones using pairs of examples |
| Low-Rank Adaptation (LoRA) | Low-Rank Adaptation | a fine-tuning method that adds small trainable matrices on top of frozen base weights, using far less memory than full training |
| Best Match 25 (BM25) | Best Match 25 | a document ranking algorithm that scores relevance based on term frequency and how rare a term is across all documents |
| Reciprocal Rank Fusion (RRF) | Reciprocal Rank Fusion | a method for combining two ranked lists into one by summing the reciprocals of each item's rank in each list |
| Key-Value cache (KV cache) | Key-Value cache | a store of previously computed key and value tensors so the model does not recompute them for tokens it has already seen |
| Feed-Forward Network (FFN) | Feed-Forward Network | the two-layer neural network that follows each attention block; it processes each token's representation independently |
| Swish-Gated Linear Unit (SwiGLU) | Swish-Gated Linear Unit | an activation function that uses a learnable gate to control how much information passes through each layer |
| Term Frequency — Inverse Document Frequency (TF-IDF) | Term Frequency — Inverse Document Frequency | an older scoring formula for document relevance; BM25 replaced it by adding length normalization |
| Operation context (scope context) | Operation context | a thread-local or async-local store that holds the current tenant or session identifier without passing it as a parameter |

In code, variable and method names must use the full descriptive name:
- `bpe` → `byteTokenizer` or `byteEncodingTokenizer`
- `hnsw` → `hierarchicalGraphIndex`
- `kvCache` → `keyValueCache`
- `rrfScore` → `reciprocalRankFusionScore`

---

## Part 5 — Sub-Project Plans (updated with coding rules, fabric, comments, full names)

All sub-project plans from version 1 remain in force. The additions in this section describe what changes in each sub-project due to the three new requirements.

---

### Sub-project 01 — Byte Pair Encoding Tokenizer

**New fabric interface:** `ITokenizerProvider` (defined in Part 2 above). The `BpeTokenizer` class is a concrete implementation of this interface. No caller may reference `BpeTokenizer` directly — all callers use `ITokenizerProvider`.

**New file to add:** `Fabrics/ITokenizerProvider.cs` — the interface. `Implementations/BpeTokenizerProvider.cs` — the concrete class that wraps `BpeTokenizer` and implements the interface.

**Coding rules that apply:**
- Coding rule 3: every public method returns `OperationResult<T>`. `Train`, `Encode`, `Decode`, `Save`, `Load` all return result objects.
- Coding rule 4: `BpeTokenizerProvider` extends `BaseService`.
- Coding rule 5: if training context (e.g. which training run triggered this) is needed, it is read from `IOperationContext`, not passed as a parameter.
- Coding rule 7: `Train` accepts an `operationId` and checks the idempotency store before starting.
- Coding rule 8: vocabulary is saved to disk before any "training complete" event is dispatched.
- Coding rule 9: the "training complete" event is wrapped in `StandardEvent`.

**Comment requirement:** every line in `BpeTokenizer.cs`, `BpeTokenizerProvider.cs`, `VocabEntry.cs`, `MergeTable.cs`, and `RegexSplitter.cs` must have a plain-language comment. The comment explains what the line does, not just what it says.

**Abbreviation check:** The file must never contain the string "BPE" without being followed by the full form "(Byte Pair Encoding — an algorithm that merges frequent pairs of bytes)" in the comment on the same line.

---

### Sub-project 02 — Dataset Pipeline

**New fabric interface:** `IDatasetStorage` — an interface for reading and writing token shard files. Concrete implementations: `NpyFileDatasetStorage` (production) and `InMemoryDatasetStorage` (tests).

```csharp
// IDatasetStorage hides the details of how token shards are saved and loaded.
// The DatasetBuilder talks to IDatasetStorage — it does not know whether
// the data goes to NumPy files, to memory, or somewhere else.
public interface IDatasetStorage
{
    // Save a chunk of tokens (unsigned 16-bit integers) to a named shard
    // (a shard — a file with a fixed number of tokens, part of a large dataset)
    Task<OperationResult<bool>> SaveShardAsync(string shardName, ushort[] tokens);

    // Load a previously saved shard back into memory
    Task<OperationResult<ushort[]>> LoadShardAsync(string shardName);

    // Return the names of all shards currently in storage
    Task<OperationResult<List<string>>> ListShardsAsync();
}
```

**Coding rules:** same as sub-project 01. `DatasetBuilder` and `DataLoader` extend `BaseService`. All public methods return `OperationResult<T>`.

**Comment requirement:** every line in every file in this sub-project carries a plain-language comment, including the NumPy file format (NumPy — a binary storage format for arrays of numbers, created for the Python library) header writing code.

---

### Sub-project 03 — Transformer Model

**New fabric interface:** `IModelProvider` (defined in Part 2). The `LlmModel` class is a concrete implementation detail. All callers use `IModelProvider`.

**New file to add:** `Fabrics/IModelProvider.cs`. `Implementations/TorchSharpModelProvider.cs` — wraps `LlmModel` and implements the interface.

**Coding rules:**
- Coding rule 1: `ModelConfig` is a `record` with only primitive fields. No typed data classes.
- Coding rule 3: `TorchSharpModelProvider.ForwardAsync`, `LoadWeightsAsync`, `SaveWeightsAsync` all return `OperationResult<T>`.
- Coding rule 4: `TorchSharpModelProvider` extends `BaseService`.

**Comment requirement example for the attention mechanism:**

```csharp
// This method computes which tokens should "pay attention" to which other tokens.
// The idea: every token has three views of itself — what it's looking for (Query),
// what it offers to others (Key), and the actual information it carries (Value).
// The output is a weighted sum of all Values, where the weights come from
// how well each Query matches each Key.
public override Tensor forward(Tensor inputTensor)
{
    // Project the input into the query space — what is each token looking for?
    // The query projection multiplies the input by a learned weight matrix
    var queryVectors = queryProjection.forward(inputTensor);

    // Project the input into the key space — what does each token offer?
    var keyVectors = keyProjection.forward(inputTensor);

    // Project the input into the value space — what does each token actually carry?
    var valueVectors = valueProjection.forward(inputTensor);

    // Apply rotary position encoding (rotary encoding of position)
    // to both query and key vectors so the model knows which token is at which position
    (queryVectors, keyVectors) = rotaryEncoding.ApplyToQueryAndKey(queryVectors, keyVectors);

    // Compute raw attention scores: how much does each query match each key?
    // Dividing by the square root of the head dimension prevents scores from
    // growing too large and making the softmax output collapse to one-hot
    var attentionScores = torch.matmul(queryVectors, keyVectors.transpose(-2, -1))
                              / MathF.Sqrt(headDimension);

    // Apply the causal mask: a token may not look at future tokens.
    // We set future positions to negative infinity so softmax assigns them zero weight.
    attentionScores = attentionScores.masked_fill(futureMask, float.NegativeInfinity);

    // Convert raw scores to weights that sum to 1 across all attended positions
    // (softmax — a normalization function: turns arbitrary numbers into probabilities)
    var attentionWeights = torch.softmax(attentionScores, dim: -1);

    // Compute the weighted sum of value vectors using the attention weights
    var attendedValues = torch.matmul(attentionWeights, valueVectors);

    // Project the attended values back to the model's hidden dimension
    return outputProjection.forward(attendedValues);
}
```

**Abbreviation check:** `RMS` → must appear as `Root Mean Square normalization (RMS normalization)` in comments. `RoPE` → `Rotary Position Embedding (RoPE)`. `GQA` → `Grouped Query Attention (GQA)`. `SwiGLU` → `Swish-Gated Linear Unit (SwiGLU)`.

---

### Sub-project 04 — Training Loop

**New fabric interface:** `ICheckpointStorage` — hides where checkpoints are saved.

```csharp
// ICheckpointStorage decides where model snapshots are kept.
// The Trainer only knows "save" and "load" — it does not care about
// whether snapshots go to disk, to cloud storage, or to memory during tests.
public interface ICheckpointStorage
{
    // Save a named snapshot of model weights and optimizer state
    Task<OperationResult<bool>> SaveAsync(string checkpointName, Dictionary<string, object> data);

    // Load a previously saved snapshot by name
    Task<OperationResult<Dictionary<string, object>>> LoadAsync(string checkpointName);

    // List all available checkpoint names
    Task<OperationResult<List<string>>> ListAsync();
}
```

**Coding rules:**
- Coding rule 7: `RunTrainingStepAsync` accepts an `operationId` and skips work if already done.
- Coding rule 8: the checkpoint is saved before the "step complete" event is dispatched.
- Coding rule 9: "step complete" event uses `StandardEvent`.

**Comment requirement:** the AdamW optimizer (the Adam optimizer with weight decay — an algorithm that updates the weights of a neural network, adapting the learning rate separately for each weight) update rule must be commented line by line, explaining what each term in the formula does in plain language.

---

### Sub-project 05 — Text Embeddings

**Fabric interface:** `IEmbeddingProvider` — hides the model behind an interface.

```csharp
// IEmbeddingProvider turns text into a numeric vector.
// Any class that needs to compare texts by meaning uses this interface.
// The interface does not say how the vector is computed —
// it could be TorchSharp, a remote API, or a precomputed lookup table.
public interface IEmbeddingProvider
{
    // Convert a single piece of text into a fixed-length numeric vector
    // The vector length depends on the underlying model configuration
    Task<OperationResult<float[]>> EmbedAsync(string text);

    // Convert multiple pieces of text in one batch (faster than calling EmbedAsync in a loop)
    Task<OperationResult<float[][]>> EmbedBatchAsync(IList<string> texts);
}
```

**Comment requirement:** `MeanPooling` and the L2 normalization (L2 normalization — dividing a vector by its length so that all vectors have the same length equal to 1) must be commented so a reader understands the math without knowing linear algebra.

---

### Sub-project 06 — Vector Search

**Fabric interface:** `IVectorStorage` (defined in Part 2 above). Two concrete implementations: `BruteForceVectorStorage` and `HierarchicalGraphVectorStorage`.

**Coding rule 2 applies:** the search filter (top-K value, distance metric) is built by `BuildVectorSearchFilter(input)`, not hardcoded at the call site.

**Comment requirement:** the Hierarchical Navigable Small World (HNSW — a hierarchical navigable small-world graph) insertion algorithm must be commented step by step. Every graph traversal decision must explain in plain language why it moves to a particular node.

---

### Sub-project 07 — Knowledge Graph Engine

**Fabric interface:** `IGraphStorage` (defined in Part 2 above).

**Coding rules:**
- Coding rule 1: graph nodes are stored as `Dictionary<string, object>`, not as typed `GraphNode` classes. The record types `GraphNode` and `GraphEdge` are allowed only as lightweight value carriers between the storage and the caller — they must not contain business logic.
- Coding rule 2: the PageRank personalization dictionary is built by `BuildPersonalizationWeights(seedNodes)`, not assembled inline.

**Comment requirement:** the PageRank (an algorithm for ranking graph nodes — the more important nodes that link to a node, the more important it is) power iteration loop must be commented so a reader understands what "distributing importance across the graph" means without knowing graph theory.

---

### Sub-project 08 — Graph Retrieval-Augmented Generation

**What Retrieval-Augmented Generation means (plain language):** instead of asking the language model a question and hoping it remembers the answer from training, we first search our own codebase for relevant pieces of code, then include those pieces in the question we send to the model. The model now has the actual code in front of it and can answer accurately.

**Fabric interfaces used:** `IVectorStorage`, `IGraphStorage`, `IEmbeddingProvider`.

**No new fabric interface needed** — all three already exist.

**Coding rules:**
- Coding rule 8: when indexing a file, the graph nodes and edges are stored (via `IGraphStorage`) before any "file indexed" event is dispatched.
- Coding rule 9: the "file indexed" event uses `StandardEvent`.

**Comment requirement:** the hybrid retrieval pipeline (local vector search + global graph traversal) must be commented so a reader understands what each stage contributes to the final answer context.

---

### Sub-project 09 — Token Generation (Inference)

**What inference means (plain language):** using the already-trained model to generate new text. The model looks at all the tokens so far and predicts which token most likely comes next. We pick one, add it to the list, and ask again — until we have the full response.

**Fabric interface:** `IModelProvider` (already defined). The `TextGenerator` receives an `IModelProvider` — it does not know whether the model is loaded locally or called remotely.

**Coding rules:**
- Coding rule 7: `GenerateAsync` accepts an `operationId`. If the same prompt with the same `operationId` is submitted twice, the second call returns the cached result.
- Coding rule 9: the "generation complete" event uses `StandardEvent`.

**Comment requirement:** the Key-Value cache (key-value cache — a store of the model's intermediate results so they are not recomputed from scratch for each new token) must be commented so a reader understands what is being cached and why it makes generation faster.

---

### Sub-project 10 — Low-Rank Adaptation Fine-Tuning

**What Low-Rank Adaptation (LoRA) means (plain language):** instead of changing all 14 billion weights of the model when adapting it to a new task, LoRA adds two tiny extra matrices (low-rank matrices — small matrices whose product mimics a large change in the weights) to specific layers. Only these tiny matrices are trained. This uses 100 times less memory.

**Fabric interface:** `IModelProvider` (already defined). The `LoraAdaptedModelProvider` wraps an existing `IModelProvider` and adds the Low-Rank Adaptation matrices.

**Coding rule 8:** the Low-Rank Adaptation adapter weights are saved before any "fine-tuning complete" event is dispatched.

**Comment requirement:** the weight merging operation (when Low-Rank Adaptation matrices are folded back into the base weights for fast inference) must be explained in plain language inside the code.

---

### Sub-project 11 — Direct Preference Optimization Training

**What Direct Preference Optimization means (plain language):** after base training the model generates text but does not know what "good" means. Direct Preference Optimization teaches it by showing pairs: here is a good answer, here is a bad answer — learn to prefer the good one. No reward model is needed; the learning signal comes directly from the pairs.

**Fabric interfaces used:** `IModelProvider` (for both the policy model and the frozen reference model), `IDatasetStorage` (for loading preference pairs), `ICheckpointStorage` (for saving fine-tuned weights).

**Coding rules:**
- Coding rule 7: each training step is identified by an `operationId` built from the batch content hash. Duplicate batches are skipped.
- Coding rule 8: checkpoint is saved before "fine-tuning step complete" event.
- Coding rule 9: the event uses `StandardEvent`.

**Comment requirement:** the Direct Preference Optimization loss formula (loss formula — a measure of how much the model currently prefers the bad answer over the good one) must be explained term by term in plain language inside the code, not just in a comment block above the method.

---

### Sub-project 12 — Full-Text Search Engine

**What the inverted index means (plain language):** a dictionary where each unique word is the key, and the value is a list of all documents containing that word. To find documents about "authentication", you look up "authentication" in the dictionary and get the list immediately — instead of reading every document.

**Fabric interface:** `IIndexStorage` (defined in Part 2 above).

**Coding rules:**
- Coding rule 2: the BM25 (Best Match 25 — an algorithm that ranks documents by relevance to the query) scoring parameters (k1 and b constants, see comment below) are not hardcoded at the call site — they are built by `BuildScoringParameters(config)`.
- Coding rule 8: documents are saved to `IIndexStorage` before any "document indexed" event is dispatched.

**Comment requirement:** every step of the BM25 formula must be explained in plain language. Example:

```csharp
// k1 controls how quickly the benefit of repeating a term diminishes.
// A value of 1.2 means: if a term appears once, it helps a lot.
// If it appears a second time, it still helps, but less.
// By the fifth occurrence, extra occurrences barely matter.
private const float TermSaturationConstant = 1.2f; // known in literature as "k1"

// b controls how much we penalize long documents.
// A value of 0.75 means: a document twice as long as average
// gets a moderate penalty — we assume some of its length
// is padding rather than extra relevance.
private const float LengthNormalizationFactor = 0.75f; // known in literature as "b"
```

**Abbreviation check:** `BM25` → `Best Match 25 (BM25)`. `Reciprocal Rank Fusion (RRF)` → `Reciprocal Rank Fusion (a method for merging two ranked lists into one)`. `TF` → `term frequency (how often a term appears in a document)`. `IDF` → `inverse document frequency (rare words are more valuable than frequent ones)`.

---

## Part 6 — Updated Summary Table

| Sub-project | Name | Dependencies | Fabric interface added | Days |
|---|---|---|---|---|
| 01 | Byte Pair Encoding Tokenizer | none | ITokenizerProvider | 4 |
| 02 | Dataset Pipeline | 01 | IDatasetStorage | 3 |
| 03 | Transformer Model | none | IModelProvider | 7 |
| 04 | Training Loop | 02, 03 | ICheckpointStorage | 4 |
| 05 | Text Embeddings | 01, 03 | IEmbeddingProvider | 2 |
| 06 | Vector Search | none | IVectorStorage | 5 |
| 07 | Knowledge Graph Engine | 05, 06 | IGraphStorage | 4 |
| 08 | Graph Retrieval-Augmented Generation | 05, 06, 07 | (uses existing interfaces) | 5 |
| 09 | Token Generation | 01, 03 | (uses IModelProvider) | 3 |
| 10 | Low-Rank Adaptation Fine-Tuning | 03, 04 | (uses IModelProvider) | 3 |
| 11 | Direct Preference Optimization Training | 04, 10 | (uses existing interfaces) | 4 |
| 12 | Full-Text Search Engine | 01, 05, 06 | IIndexStorage | 11 |
| **Total** | | | | **55 days** |

---

## Part 7 — Pre-commit Checklist for Every File

Before any file is considered complete, the author must check:

```
□ Every public method returns OperationResult<T> (coding rule 3)
□ Every service class extends BaseService (coding rule 4)
□ No tenantId, sessionId, or scopeId parameter in any method signature (coding rule 5)
□ No new() call on a fabric implementation inside a service — only injected interfaces (fabric-first)
□ Every line carries a plain-language comment
□ No abbreviation appears without the full form and plain-language explanation on the same line
□ dotnet build exits 0, no warnings
□ All unit tests pass
```

---

*Plan version 2.0 — adds coding rules (Genie DNA), fabric-first interfaces, comment standard, and full abbreviation expansion. Covers all 12 sub-projects. Does not touch XIIGen work.*
