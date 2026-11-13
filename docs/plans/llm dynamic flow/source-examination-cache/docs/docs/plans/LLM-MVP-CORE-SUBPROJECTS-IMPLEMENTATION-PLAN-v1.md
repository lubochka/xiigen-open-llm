# LLM MVP Core — Plan for Implementing All 12 Sub-Projects
## Solution: LlmFromScratch.sln — Implementation Plan v1.0

**Project:** llm-mvp-core  
**Language:** C# / .NET 8  
**Constraint:** No external NuGet packages except TorchSharp for matrix operations  
**Document purpose:** Step-by-step implementation plan for each of the 12 sub-projects as independent class libraries within the same solution  
**This document is separate from all XIIGen work.**

---

## How to read this plan

Each sub-project section has the same structure:

- **Goal** — what the project delivers in one sentence
- **Dependency on prior projects** — which projects must be complete before this one starts
- **Implementation phases** — ordered phases, each with: what to write, what tests to produce, and what the phase exit criterion is
- **Acceptance criteria** — how to know the sub-project is done
- **Estimated days** — conservative range

**Gate definitions used throughout:**

- Gate A (automated) — all unit tests pass, `dotnet build` exits 0, no compiler warnings
- Gate B (cross-check) — implementation matches the reference algorithm from the stated open-source source; at least one correctness test verifies output matches the Python reference
- Gate C (approval) — explicit "yes" from Luba before moving to the next sub-project

Every sub-project requires Gate A and Gate B before Gate C is requested. No chaining — one sub-project completes fully before the next begins.

---

## Dependency map (build order)

```
01 Tokenizer
    ↓
02 Dataset Pipeline      (needs 01)
    ↓
03 Transformer Model     (independent — can start in parallel with 02)
    ↓
04 Training Loop         (needs 02, 03)
    ↓
05 Embeddings            (needs 01, 03)
    ↓
06 Vector Search         (independent — pure math, no dependencies)
    ↓
07 Graph Engine          (needs 05, 06)
    ↓
08 Graph RAG             (needs 05, 06, 07)
    ↓
09 Inference             (needs 01, 03)
    ↓
10 LoRA                  (needs 03, 04)
    ↓
11 DPO                   (needs 04, 10)
    ↓
12 Search Engine         (needs 01, 05, 06)
```

Parallel opportunity: projects 02 and 03 can be worked on in parallel once project 01 is complete. Projects 06 and 09 can be started independently as soon as their prerequisites are clear.

---

## Sub-project 01 — BPE Tokenizer

**Goal:** Produce a self-contained BPE tokenizer that trains from raw text, encodes text to token lists, and decodes token lists back to text, with save/load for the vocabulary.

**Dependencies:** None. This is the foundation for every other sub-project.

**Reference source:** `https://github.com/karpathy/minbpe` — MIT license, Python, ~150 lines per file.

### Phase 1 — Vocabulary and merge table (1 day)

What to write:
- `VocabEntry.cs` — record holding token index and byte representation
- `MergeTable.cs` — dictionary mapping token-pair to new token index
- Unit tests: add 10 entries, verify lookup by index and by pair

Phase exit: `dotnet test` passes, all tests green.

### Phase 2 — Training algorithm (1–2 days)

What to write:
- `BpeTokenizer.cs` — `Train(string text, int vocabSize)` method
  - Step 1: convert text to byte sequence (each byte is token 0–255)
  - Step 2: count all adjacent token pairs
  - Step 3: merge the most frequent pair into a new token
  - Step 4: record the merge in the merge table
  - Step 5: repeat until vocabulary reaches target size

Unit tests:
- Train on a 20-word string, verify vocabulary grew beyond 255
- Verify the most frequent pair in the training text appears in the merge table
- Verify vocabulary size equals the requested size

Phase exit: Gate A. Training produces a deterministic vocabulary given the same input.

### Phase 3 — Encode and decode (1 day)

What to write:
- `BpeTokenizer.Encode(string text)` — apply merges in training order, return token list
- `BpeTokenizer.Decode(List<int> tokens)` — reverse vocab lookup, return string

Unit tests:
- Round-trip: `Decode(Encode(text)) == text` for 5 different strings
- Verify that encoding a word seen during training produces fewer tokens than the raw byte count

Phase exit: Gate A. Round-trip tests pass.

### Phase 4 — Persistence (0.5 day)

What to write:
- `BpeTokenizer.Save(string path)` — write vocabulary and merge table to JSON
- `BpeTokenizer.Load(string path)` — reconstruct tokenizer from JSON

Unit tests:
- Save and reload a trained tokenizer, verify Encode output is identical before and after reload

Phase exit: Gate A.

### Phase 5 — Regex splitter (0.5 day)

What to write:
- `RegexSplitter.cs` — pre-tokenize text into words before byte-level BPE (matches GPT-2 pattern: words, contractions, punctuation)

Unit tests:
- Verify `"don't"` splits into `["don", "'t"]` not a single token
- Verify whitespace is handled correctly

Phase exit: Gate A.

**Gate B check:** Encode the string `"hello world hello"` by hand following the merge table from the training trace. Verify the unit test produces the same token sequence.

**Acceptance criteria:**
- All 5 test files pass
- Round-trip correctness on 10 test strings
- Save/load produces identical outputs

**Estimated days:** 4

---

## Sub-project 02 — Dataset Pipeline

**Goal:** Produce a pipeline that reads source code files, tokenizes them into a flat token stream, chunks the stream into fixed-length windows, and saves chunks to disk in NumPy `.npy` format.

**Dependencies:** Sub-project 01 must be complete (uses `BpeTokenizer`).

**Reference source:** `https://github.com/karpathy/nanoGPT` — `data/shakespeare_char/prepare.py`, MIT, ~50 lines.

### Phase 1 — NumPy writer and reader (1 day)

What to write:
- `NpyWriter.cs` — writes `uint16` arrays to `.npy` format (128-byte header + raw bytes)
- `NpyReader.cs` — reads `.npy` files back to `ushort[]`

Unit tests:
- Write a 1000-element array, read it back, verify all values match
- Verify the 6-byte magic number and version bytes at the start of the file

Phase exit: Gate A.

### Phase 2 — Data loader (1 day)

What to write:
- `DataLoader.cs` — reads `.npy` shards from a directory, returns batches of `(inputs, targets)` where targets are inputs shifted one position right

Unit tests:
- Given a shard with tokens `[1, 2, 3, 4, 5]` and sequence length 3, verify the first batch is `inputs=[1,2,3]`, `targets=[2,3,4]`
- Verify loading wraps around when the end of a shard is reached

Phase exit: Gate A.

### Phase 3 — Dataset builder (1–2 days)

What to write:
- `DatasetBuilder.cs` — walks a source directory, tokenizes every `.ts`, `.cs`, `.py` file, appends an end-of-file token between files, accumulates tokens until shard size is reached, then writes a shard

Unit tests:
- Build from a temp directory with 3 small test files, verify at least one `.npy` shard is produced
- Verify the shard contains the end-of-file token at the expected position

Phase exit: Gate A.

**Gate B check:** Manually tokenize the first test file. Verify the first N entries in the produced shard match the expected token sequence.

**Acceptance criteria:**
- All test files pass
- A test directory with 3 files produces a valid shard that the DataLoader can read
- Round-trip: tokens in → shard → DataLoader batch → correct inputs and targets

**Estimated days:** 3

---

## Sub-project 03 — Transformer Model

**Goal:** Produce the full neural network architecture: RMS normalization, rotary position embeddings, grouped-query attention, feed-forward with SwiGLU activation, transformer blocks, and the top-level language model.

**Dependencies:** TorchSharp only. No dependency on other sub-projects.

**Reference source:** `https://github.com/karpathy/nanoGPT` — `model.py`, MIT, ~300 lines.

### Phase 1 — Configuration (0.5 day)

What to write:
- `Config/ModelConfig.cs` — record holding all hyperparameters: number of layers, hidden dimension, number of attention heads, number of key-value heads, vocabulary size, sequence length, feed-forward multiplier

Unit tests:
- Verify that a default config produces a model with the expected parameter count formula (can be checked algebraically without running the model)

Phase exit: Gate A.

### Phase 2 — RMS normalization (0.5 day)

What to write:
- `Layers/RmsNorm.cs` — TorchSharp `Module<Tensor, Tensor>` subclass
  - `gamma` parameter (learnable scale, one per dimension)
  - `forward(x)`: compute `x / sqrt(mean(x^2) + eps) * gamma`

Unit tests:
- Verify that a tensor of all-ones produces output of all-ones (gamma initialized to ones, eps near zero)
- Verify output shape matches input shape

Phase exit: Gate A.

### Phase 3 — Rotary position embeddings (1 day)

What to write:
- `Layers/RotaryEmbedding.cs`
  - `GetFrequencies(int seqLen, Device device)` — returns cosine and sine matrices of shape `(seqLen, headDim/2)`
  - `Apply(Tensor x, Tensor cos, Tensor sin)` — rotates query or key tensors

Unit tests:
- Verify that rotating a tensor and then un-rotating it (negative angle) produces the original tensor
- Verify output shape is unchanged

Phase exit: Gate A.

### Phase 4 — Causal self-attention (2 days)

What to write:
- `Layers/CausalSelfAttention.cs`
  - Four `Linear` projections: query, key, value, output
  - `RotaryEmbedding` instance
  - `forward(x)`:
    - Project to query, key, value
    - Apply rotary embeddings to query and key
    - Compute scores: `query @ key^T / sqrt(headDim)`
    - Apply causal mask: set future positions to negative infinity
    - Softmax
    - Weighted sum of values
    - Project output

Unit tests:
- Verify output shape matches input shape
- Verify that with a causal mask, each position only attends to itself and earlier positions (test with a hand-constructed mask)

Phase exit: Gate A.

### Phase 5 — Feed-forward (0.5 day)

What to write:
- `Layers/SwiGLUFeedForward.cs`
  - Three `Linear` layers: gate, up, down
  - `forward(x)`: `(silu(x @ gate) * (x @ up)) @ down`

Unit tests:
- Verify output shape matches input shape
- Verify that output changes when weights change (not a dead layer)

Phase exit: Gate A.

### Phase 6 — Transformer block and full model (1 day)

What to write:
- `TransformerBlock.cs` — wraps attention + feed-forward with residual connections and RMS normalization
- `LlmModel.cs` — token embedding table, list of transformer blocks, final normalization, language model head (`Linear` from hidden dimension to vocabulary size)

Unit tests:
- Forward pass on a batch of 2 sequences of length 10 produces output of shape `(2, 10, vocabSize)`
- Verify gradients flow through all layers (call `loss.backward()` and check no parameter has a null gradient)

Phase exit: Gate A.

**Gate B check:** Run a forward pass and compare the output distribution (softmax over vocabulary) to a reference value computed by hand for a minimal configuration (2 layers, 8 dimensions, 2 heads, vocabulary of 10).

**Acceptance criteria:**
- All layer tests pass
- Full model forward pass produces the correct output shape
- Gradients flow through all parameters

**Estimated days:** 7

---

## Sub-project 04 — Training Loop

**Goal:** Produce an optimizer (AdamW), a learning rate scheduler (cosine with warmup), a trainer that runs one step and accumulates gradients, and checkpoint save/load.

**Dependencies:** Sub-projects 02 and 03 must be complete.

**Reference source:** `https://github.com/karpathy/nanoGPT` — `train.py`, MIT, ~300 lines.

### Phase 1 — AdamW optimizer (1 day)

What to write:
- `Optimizers/AdamW.cs`
  - Stores moment estimates (`m` and `v`) per parameter
  - `Step(parameters)`: applies the AdamW update rule with weight decay

Unit tests:
- Verify that a single parameter with a constant gradient decreases monotonically for 10 steps
- Verify weight decay pulls a parameter toward zero even when gradient is zero

Phase exit: Gate A.

### Phase 2 — Learning rate scheduler (0.5 day)

What to write:
- `Schedulers/CosineWithWarmup.cs`
  - Linear warmup from 0 to `maxLr` over `warmupSteps`
  - Cosine decay from `maxLr` to `minLr` after warmup

Unit tests:
- Verify step 0 returns 0
- Verify step `warmupSteps` returns `maxLr`
- Verify learning rate is monotonically decreasing after warmup

Phase exit: Gate A.

### Phase 3 — Trainer (2 days)

What to write:
- `Trainer.cs`
  - `TrainStepAsync(model, dataLoader)` — forward pass, cross-entropy loss, backward pass, gradient clipping, optimizer step, zero gradients
  - `SaveCheckpoint(model, optimizer, step, dir)` — serialize weights and optimizer state
  - `LoadCheckpoint(model, optimizer, path)` — restore from disk

Unit tests:
- Verify that loss decreases over 10 training steps on a tiny dataset (5 token sequences)
- Verify that a saved and loaded checkpoint produces the same loss on the same batch

Phase exit: Gate A.

### Phase 4 — Gradient accumulation (0.5 day)

What to write:
- `TrainingConfig.cs` — includes `gradAccumSteps` field
- Update `Trainer` to accumulate gradients over `gradAccumSteps` micro-batches before calling the optimizer step

Unit tests:
- Verify that training with accumulation over 4 steps produces the same weight update as training with a batch 4x larger (within floating point tolerance)

Phase exit: Gate A.

**Gate B check:** Train for 50 steps on a 100-token sequence. Verify loss decreases from its initial value by at least 10%.

**Acceptance criteria:**
- Loss decreases over training
- Checkpoint round-trip produces identical model outputs
- Gradient accumulation produces the same result as a larger batch

**Estimated days:** 4

---

## Sub-project 05 — Embeddings

**Goal:** Produce a module that converts text into fixed-length embedding vectors using a trained transformer encoder with mean pooling and L2 normalization.

**Dependencies:** Sub-projects 01 and 03 must be complete.

**Reference source:** `https://github.com/UKPLab/sentence-transformers` — `Pooling.py`, Apache 2.0, ~150 lines.

### Phase 1 — Mean pooling (1 day)

What to write:
- `MeanPooling.cs`
  - `Pool(Tensor tokenVectors, Tensor attentionMask)` — masks out padding positions, averages the remaining token vectors
  - `Normalize(Tensor vectors)` — divides each vector by its L2 norm

Unit tests:
- Verify that pooling a sequence with half the positions masked gives the same result as manually averaging only the unmasked positions
- Verify that normalized vectors all have L2 norm of exactly 1.0 (within 1e-5)

Phase exit: Gate A.

### Phase 2 — Embedding model (1 day)

What to write:
- `EmbeddingModel.cs`
  - `Embed(string text)` — tokenize, run through encoder (without the language model head), pool, normalize, return `float[]`
  - `EmbedBatch(IList<string> texts)` — process multiple texts together

Unit tests:
- Verify that two identical strings produce identical vectors
- Verify that two semantically opposite strings (e.g., "cat" vs "automobile") produce vectors with cosine similarity less than semantically similar strings (e.g., "cat" vs "kitten") — after a minimal training run on paired examples
- Verify output dimension matches model's hidden dimension

Phase exit: Gate A.

**Gate B check:** Embed 3 strings by hand (trace through the mean pooling formula manually). Verify the unit test produces the same vector.

**Acceptance criteria:**
- Identical inputs produce identical outputs
- Normalized outputs have unit L2 norm
- Batch embedding produces the same result as individual embeddings

**Estimated days:** 2

---

## Sub-project 06 — Vector Search

**Goal:** Produce a brute-force vector index for correctness verification and an HNSW index for approximate nearest-neighbor search, with cosine similarity as the distance function.

**Dependencies:** None. Pure mathematics, no other sub-projects required.

**Reference source:** `https://github.com/nmslib/hnswlib` — Apache 2.0, C++. Read `bruteforce.h` first, then `hnswalg.h`.

### Phase 1 — Distance functions (0.5 day)

What to write:
- `Distances.cs`
  - `CosineSimilarity(float[] a, float[] b)`
  - `EuclideanDistance(float[] a, float[] b)`
  - `InnerProduct(float[] a, float[] b)`

Unit tests:
- Verify cosine similarity of a vector with itself is 1.0
- Verify cosine similarity of orthogonal vectors is 0.0
- Verify cosine similarity of a vector and its negation is -1.0

Phase exit: Gate A.

### Phase 2 — Brute-force index (1 day)

What to write:
- `BruteForceIndex.cs`
  - `Add(float[] vector, string id)`
  - `Search(float[] query, int topK)` — linear scan, return topK most similar with scores

Unit tests:
- Add 100 random vectors, search for the most similar, verify it returns the inserted vector with score 1.0 (exact match)
- Verify topK results are sorted by descending similarity

Phase exit: Gate A.

### Phase 3 — HNSW index (3 days)

What to write:
- `HnswIndex.cs`
  - Hierarchical graph with configurable `M` (connections per node) and `efConstruction` (build-time search width)
  - `Add(float[] vector, string id)` — insert with random level assignment and neighbor connection
  - `Search(float[] query, int topK)` — greedy descent from top level to level 0, then beam search at level 0
  - `Save(string path)` and `Load(string path)` — serialize/deserialize the full graph

Unit tests:
- Add 1000 random vectors, search for 10 known vectors, verify all 10 are found in the topK results (recall test)
- Verify that HNSW results match brute-force results for the same 50 random queries (within 90% recall)
- Verify save/load produces identical search results

Phase exit: Gate A. Recall at 90% on random data.

**Gate B check:** Trace through 3 insert operations by hand on a minimal graph (5 nodes, M=2). Verify the unit test produces the same graph structure.

**Acceptance criteria:**
- HNSW recall ≥ 90% versus brute force on 1000-vector test set
- Save/load round-trip is lossless
- Search results are sorted by descending similarity

**Estimated days:** 5

---

## Sub-project 07 — Graph Engine

**Goal:** Produce a directed weighted knowledge graph with add/query operations and a PageRank calculator supporting both standard and personalized modes.

**Dependencies:** Sub-projects 05 and 06 must be complete (embeddings stored on nodes).

**Reference source:** `https://github.com/networkx/networkx` — `pagerank_alg.py`, BSD 3-Clause, ~80 lines.

### Phase 1 — Graph data model (0.5 day)

What to write:
- `Models/GraphNode.cs` — record with id, name, type, file path, signature, raw text, embedding vector, PageRank score
- `Models/GraphEdge.cs` — record with from-id, to-id, relation type, weight

Unit tests:
- Verify node equality and immutability (records are equal by value)
- Verify edge weight defaults to 1.0 when not specified

Phase exit: Gate A.

### Phase 2 — Knowledge graph operations (1 day)

What to write:
- `KnowledgeGraph.cs`
  - `AddNode(GraphNode)` and `AddEdge(GraphEdge)`
  - `GetNeighbors(string nodeId, int hops)` — BFS up to the given hop count, returns all reachable nodes
  - `InEdges(string nodeId)` and `OutEdges(string nodeId)`

Unit tests:
- Add 5 nodes and 4 edges, verify `GetNeighbors` at 1 hop returns the correct neighbors
- Verify `GetNeighbors` at 2 hops returns nodes 2 edges away
- Verify in-edges and out-edges are correctly separated

Phase exit: Gate A.

### Phase 3 — PageRank (2 days)

What to write:
- `PageRankCalculator.cs`
  - `Compute(graph, iterations)` — power iteration with damping factor 0.85
  - `ComputePersonalized(graph, personalization, iterations)` — same but teleportation is weighted by the personalization dictionary

Unit tests:
- Verify that PageRank scores sum to approximately 1.0
- Verify that a node with more in-edges has a higher rank than a node with fewer (in a hand-constructed graph)
- Verify that personalized PageRank assigns higher scores to nodes near the seed nodes

Phase exit: Gate A.

### Phase 4 — Graph serialization (0.5 day)

What to write:
- `GraphSerializer.cs` — save the full graph (nodes + edges) to JSON, load it back

Unit tests:
- Save and load a 20-node graph, verify node and edge counts match
- Verify node embedding vectors are preserved with float precision

Phase exit: Gate A.

**Gate B check:** Run standard PageRank on the hand-constructed graph from the NetworkX example. Verify scores match within 0.001.

**Acceptance criteria:**
- PageRank scores are correct on a known reference graph
- Neighbor traversal returns the right set at every hop depth
- Save/load round-trip preserves all data

**Estimated days:** 4

---

## Sub-project 08 — Graph RAG

**Goal:** Produce a two-level retrieval system: local search (vector similarity) and global search (personalized PageRank over the knowledge graph), combined into a hybrid retriever that assembles a text context for a language model.

**Dependencies:** Sub-projects 05, 06, and 07 must be complete.

**Reference source:** `https://github.com/gusye1234/nano-graphrag` — `base.py`, MIT, ~400 lines.

### Phase 1 — Indexer (2 days)

What to write:
- `Indexer.cs`
  - `IndexFileAsync(string filePath)` — parse file (use Roslyn for C#, or line-based heuristics for other languages), extract classes and methods as graph nodes, extract calls and inheritance as edges, compute embeddings for each node, insert into both the graph and the vector index
  - `ComputeRanks()` — run PageRank over the full graph after indexing

Unit tests:
- Index a single 20-line C# file with one class and one method, verify 2 nodes and 1 edge in the graph
- Verify each node has a non-null embedding vector

Phase exit: Gate A.

### Phase 2 — Local search (0.5 day)

What to write:
- `LocalSearch.cs`
  - `Search(float[] queryEmbedding, int topK)` — queries the HNSW index, maps results back to graph nodes, returns raw text of the matched nodes

Unit tests:
- Index 10 nodes, search with the embedding of one of them, verify that node appears in the results

Phase exit: Gate A.

### Phase 3 — Global search (1 day)

What to write:
- `GlobalSearch.cs`
  - `Search(List<SearchResult> localResults, int hops, int topK)` — builds personalization weights from local search scores, runs personalized PageRank, traverses the graph from the highest-ranked nodes, returns related nodes sorted by PageRank

Unit tests:
- Index a 5-node graph with clear PageRank structure, verify that global search expands to the correct neighboring nodes

Phase exit: Gate A.

### Phase 4 — Hybrid retriever (1 day)

What to write:
- `HybridRetriever.cs`
  - `RetrieveAsync(string query)` — embed the query, run local search, run global search, deduplicate results, sort by PageRank, return a `RetrievalContext` with the assembled text

Unit tests:
- Retrieve context for a query that matches 2 known nodes, verify both appear in the context
- Verify that duplicate nodes from local and global search appear only once in the result

Phase exit: Gate A.

**Gate B check:** Index a hand-crafted 10-node graph where the expected answer to a test query is a specific node 2 hops away from the best vector match. Verify the hybrid retriever includes that node in the context.

**Acceptance criteria:**
- Indexer correctly extracts nodes and edges from a C# file
- Hybrid retrieval returns relevant context for queries that require graph traversal
- Deduplication works correctly

**Estimated days:** 5

---

## Sub-project 09 — Inference

**Goal:** Produce a key-value cache for transformer inference, a set of sampling strategies, and a streaming text generator that uses the cache to generate tokens efficiently.

**Dependencies:** Sub-projects 01 and 03 must be complete.

**Reference source:** `https://github.com/karpathy/nanoGPT` — `model.py GPT.generate()`, MIT. Also `https://github.com/ggerganov/llama.cpp` — `llama-sampling.cpp`, MIT.

### Phase 1 — Key-value cache (1 day)

What to write:
- `KvCache.cs`
  - Stores key and value tensors for each layer
  - `Update(layerIdx, newKeys, newValues)` — concatenates new keys/values to the existing cache along the sequence dimension, returns the full cached tensor
  - `Clear()` — resets the cache for a new generation request

Unit tests:
- Verify that after 3 update calls, the cached keys have sequence length 3
- Verify that Clear resets all layers to empty

Phase exit: Gate A.

### Phase 2 — Sampling strategies (1 day)

What to write:
- `Sampler.cs`
  - `SampleGreedy(Tensor logits)` — argmax
  - `SampleWithTemperature(Tensor logits, float temperature)` — divide logits by temperature, softmax, sample
  - `SampleTopK(Tensor logits, int k, float temperature)` — zero out all but top-K logits, then sample
  - `SampleTopP(Tensor logits, float p, float temperature)` — sort probabilities descending, keep the minimum prefix whose sum exceeds p, sample from that prefix

Unit tests:
- Verify greedy always returns the index with the highest logit
- Verify temperature = 0.001 (near-zero) behaves like greedy
- Verify TopP with p=0.0 returns only the single most likely token
- Verify TopP with p=1.0 can return any token

Phase exit: Gate A.

### Phase 3 — Text generator (1 day)

What to write:
- `TextGenerator.cs`
  - `GenerateAsync(string prompt, int maxNewTokens, float temperature, int topK, float topP, CancellationToken)` — tokenize prompt, initialize cache, autoregressively generate tokens one at a time (only forward the new token, reuse cached keys/values), stop at end-of-sequence token, yield each decoded token as it is produced

Unit tests:
- Generate 10 tokens from a minimal trained model, verify exactly 10 tokens are produced
- Verify that cancellation stops generation mid-sequence
- Verify that generation with the same seed and temperature produces the same output (deterministic path through greedy)

Phase exit: Gate A.

**Gate B check:** Run the text generator on the model trained in sub-project 04. Verify that output is coherent (not all the same token) and that generation is faster with the cache than without it.

**Acceptance criteria:**
- All sampling strategies produce valid token indices
- Generator streams tokens via `IAsyncEnumerable`
- Cache produces faster generation than naive re-computation

**Estimated days:** 3

---

## Sub-project 10 — LoRA Adaptation

**Goal:** Produce LoRA adapters that wrap existing `Linear` layers, a tool to apply and remove adapters from a model, and save/load for adapter weights only.

**Dependencies:** Sub-projects 03 and 04 must be complete.

**Reference source:** `https://github.com/microsoft/LoRA` — `loralib/layers.py`, MIT, ~200 lines.

### Phase 1 — LoRA linear layer (1.5 days)

What to write:
- `LoraLinear.cs`
  - Holds a frozen `Linear` base layer plus two learnable matrices: A and B
  - A is initialized with small random values; B is initialized to zero (so the initial contribution of LoRA is zero)
  - `forward(x)`: base layer output plus `(x @ A^T @ B^T) * scale`
  - `MergeWeights()` — fuses LoRA weights into the base layer and returns a plain `Linear`

Unit tests:
- Verify that immediately after initialization, LoRA output equals base layer output (because B is zero)
- Verify that after one training step, output changes
- Verify that `MergeWeights` produces the same output as the un-merged LoRA (within floating point tolerance)

Phase exit: Gate A.

### Phase 2 — LoRA applicator (1 day)

What to write:
- `LoraApplicator.cs`
  - `Apply(LlmModel model, LoraConfig config)` — replace the query and value projections in every attention layer with `LoraLinear` wrappers; freeze all other parameters
  - `GetTrainableParams(LlmModel model)` — return only the A and B matrices
  - `SaveAdapter(LlmModel model, string path)` — serialize only the A and B matrices from all LoRA layers
  - `LoadAdapter(LlmModel model, string path)` — restore A and B from disk into an existing model

Unit tests:
- After applying LoRA to a model, verify that only the A and B parameters have `requires_grad = true`
- Verify that the number of trainable parameters equals `2 * numLayers * rank * (hiddenDim + hiddenDim)` (one A and one B per layer per of the two target projections)
- Verify that saving and loading the adapter produces identical output on the same input

Phase exit: Gate A.

**Gate B check:** Fine-tune a minimal model (2 layers) with LoRA for 20 steps on a 50-token sequence. Verify loss decreases. Merge weights and verify the merged model produces the same output as the unmerged one.

**Acceptance criteria:**
- Initial LoRA output matches base layer output
- Only A and B matrices are trainable
- Save/load adapter round-trip is lossless

**Estimated days:** 3

---

## Sub-project 11 — DPO Training

**Goal:** Produce a DPO trainer that computes the direct preference optimization loss from (prompt, chosen, rejected) triples and trains a policy model relative to a frozen reference model.

**Dependencies:** Sub-projects 04 and 10 must be complete.

**Reference source:** `https://github.com/eric-mitchell/direct-preference-optimization` — `trainers.py`, MIT, ~300 lines.

### Phase 1 — Log probability computation (1 day)

What to write:
- `DpoTrainer.GetLogProbs(LlmModel model, Tensor promptTokens, Tensor responseTokens)` — run a forward pass on the concatenated prompt+response, extract logits for the response positions, compute per-token log probabilities, sum them

Unit tests:
- Verify that log probability of a response decreases when the response tokens do not match the model's distribution
- Verify that the sum of log probabilities is negative (probabilities are between 0 and 1)

Phase exit: Gate A.

### Phase 2 — DPO loss (1 day)

What to write:
- `DpoTrainer.ComputeDpoLoss(chosenLogProbs, rejectedLogProbs, refChosenLogProbs, refRejectedLogProbs)` — implements the DPO loss formula: negative log sigmoid of beta times the reward difference

Unit tests:
- Verify that when chosen log probability is much higher than rejected, loss is near zero
- Verify that when rejected is more likely than chosen, loss is high
- Verify that the loss is always non-negative

Phase exit: Gate A.

### Phase 3 — Dataset and training step (1.5 days)

What to write:
- `DpoDataset.cs`
  - `LoadFromJson(string path)` — load `[{prompt, chosen, rejected}]` triples from a JSON file
  - `GetBatches(int batchSize)` — tokenize and batch

- `DpoTrainer.TrainStepAsync(DpoBatch batch)` — run four forward passes (policy chosen, policy rejected, reference chosen, reference rejected), compute the DPO loss, backward, optimizer step

Unit tests:
- Train for 10 steps on 3 DPO triples where chosen is clearly better than rejected
- Verify that after training, the policy assigns higher log probability to chosen than rejected

Phase exit: Gate A.

**Gate B check:** Train for 30 steps. Verify that `log_prob(chosen | policy) - log_prob(rejected | policy)` increases over the course of training (the implicit reward improves).

**Acceptance criteria:**
- DPO loss decreases over training
- Policy prefers chosen over rejected after fine-tuning
- Dataset loading handles JSON correctly

**Estimated days:** 4

---

## Sub-project 12 — Search Engine

**Goal:** Produce a full-text search engine with text analysis (tokenization, lowercasing, stop-word removal, stemming), an inverted index, BM25 scoring, and a hybrid search layer combining BM25 with HNSW vector search using Reciprocal Rank Fusion.

**Dependencies:** Sub-projects 01, 05, and 06 must be complete.

**Reference source:** `https://github.com/apache/lucene` — Apache 2.0, Java. Specific files: `BM25Similarity.java`, `IndexWriter.java`, `HnswGraphBuilder.java`, `StandardTokenizer.java`.

### Phase 1 — Analyzers (2 days)

What to write:
- `Analysis/IAnalyzer.cs` — interface: `Analyze(string text)` returns `IEnumerable<AnalyzedToken>`
- `Analysis/AnalyzedToken.cs` — record: term, position, start offset, end offset
- `Analysis/StandardAnalyzer.cs` — tokenize by word boundaries, lowercase, remove stop words, apply Porter stemming
- `Analysis/PorterStemmer.cs` — all 5 algorithmic steps (approximately 150 lines of suffix rules)

Unit tests:
- Verify `"Running"` and `"run"` both produce the stem `"run"` (or the same stem)
- Verify `"the"` is removed as a stop word
- Verify `"Hello, World!"` produces tokens `["hello", "world"]` (no punctuation, no stop words)
- Verify token positions are assigned sequentially starting from 0

Phase exit: Gate A.

### Phase 2 — Inverted index (3 days)

What to write:
- `Index/PostingsList.cs` — sorted list of `Posting` records (document id, term frequency, positions list); exposes document frequency count
- `Index/Posting.cs` — record: document id, frequency, positions
- `Index/InvertedIndex.cs` — `AddDocument(text, path, analyzer)` adds all analyzed tokens to the index; tracks average document length
- `Index/SegmentMerger.cs` — merge multiple `InvertedIndex` instances into one

Unit tests:
- Index 3 documents, verify that a term appearing in 2 of them has document frequency 2
- Verify positions are recorded correctly for a term appearing twice in one document
- Merge two indexes each with 2 documents; verify the merged index has 4 documents
- Verify average document length updates after each addition

Phase exit: Gate A.

### Phase 3 — BM25 scoring (1 day)

What to write:
- `Search/BM25Scorer.cs`
  - `Score(InvertedIndex index, IEnumerable<string> queryTerms)` — returns a dictionary of document id to BM25 score
  - Internal: IDF formula `log(1 + (N - df + 0.5) / (df + 0.5))`, term frequency normalization with k1=1.2 and b=0.75

- `Search/SearchEngine.cs`
  - `Index(string text, string path)` — add document
  - `Search(string query, int topK)` — analyze query, score with BM25, return top results with snippets

Unit tests:
- Index 5 documents where one clearly matches a query better than the others; verify it ranks first
- Verify that a document containing a query term 5 times ranks higher than one containing it once
- Verify that a very long document with the same term frequency is penalized relative to a shorter one (length normalization)

Phase exit: Gate A.

### Phase 4 — Hybrid search (1.5 days)

What to write:
- `Search/HybridSearch.cs`
  - `SearchAsync(string query, int topK)` — run BM25 search, run HNSW vector search (using EmbeddingModel from sub-project 05), merge results with Reciprocal Rank Fusion (RRF constant k=60), return top results sorted by combined score

Unit tests:
- Index 10 documents. One matches well on keywords, another matches well semantically but uses different words. Verify both appear in the top 5 hybrid results.
- Verify RRF scores are computed correctly for a hand-constructed pair of rankings

Phase exit: Gate A.

### Phase 5 — Persistence (1 day)

What to write:
- `Storage/IndexStorage.cs`
  - `Save(InvertedIndex index, string directory)` — writes `terms.json` (term-to-offset map), `postings.bin` (binary posting lists), `docs.json` (document metadata)
  - `Load(string directory)` — reconstructs a fully functional index

Unit tests:
- Save a 20-document index, load it back, verify document count and term count match
- Verify a search on the loaded index returns the same results as on the original

Phase exit: Gate A.

**Gate B check:** Compare BM25 scores computed by the C# implementation against scores computed by hand using the Okapi BM25 formula for a 3-document collection and a 2-term query. Verify scores match within 0.001.

**Acceptance criteria:**
- BM25 ranking is correct on a known reference collection
- Hybrid search includes both keyword-matched and semantically-matched documents
- Persistence round-trip is lossless

**Estimated days:** 11

---

## Summary table

| Sub-project | Name | Dependencies | Estimated days |
|---|---|---|---|
| 01 | BPE Tokenizer | none | 4 |
| 02 | Dataset Pipeline | 01 | 3 |
| 03 | Transformer Model | none (TorchSharp only) | 7 |
| 04 | Training Loop | 02, 03 | 4 |
| 05 | Embeddings | 01, 03 | 2 |
| 06 | Vector Search | none | 5 |
| 07 | Graph Engine | 05, 06 | 4 |
| 08 | Graph RAG | 05, 06, 07 | 5 |
| 09 | Inference | 01, 03 | 3 |
| 10 | LoRA Adaptation | 03, 04 | 3 |
| 11 | DPO Training | 04, 10 | 4 |
| 12 | Search Engine | 01, 05, 06 | 11 |
| **Total** | | | **55 days** |

---

## Parallel execution opportunity

If two developers work in parallel:

- **Track A**: 01 → 02 → 03 → 04 → 09 → 10 → 11
- **Track B**: 06 → 05 (after 01 and 03 are done) → 07 → 08 → 12 (after 05 and 06 are done)

Parallel execution compresses total calendar time to approximately 32 days.

---

## Gate protocol summary

Before declaring any sub-project complete and moving to the next:

1. **Gate A (automated):** `dotnet build` exits 0, all unit tests pass, no compiler warnings
2. **Gate B (algorithmic correctness):** at least one test verifies output matches the reference algorithm trace
3. **Gate C (approval):** explicit written "yes" from Luba

No sub-project may start until all its listed dependencies have passed Gate C.

---

*Plan version 1.0 — covers all 12 sub-projects, does not touch XIIGen work.*
