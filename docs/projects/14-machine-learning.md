# 14.MachineLearning - classic ML algorithms

Documentation status: d3c_documented_parent_review_passed  
Last verified against source: 2026-06-08  
Known gaps: the tests were not run in this documentation subphase; `state.json` is used only as historical/status context, not as runtime proof of quality.

## 0. Quick map for Luba

`14.MachineLearning` is a set of classic machine-learning algorithms written in C# without external ML frameworks. Simple numeric, text and tabular data passes through the project: points for clustering, `x/y` pairs for regression, document tokens for Naive Bayes, transactions for Apriori, states/actions for Q-Learning. Some algorithms have service wrappers with `IModelStateStorage`, `IPredictionLogger` and idempotency: `LinearRegressionService`, `NaiveBayesService`, `MultilayerPerceptronService`, `KMeansService`. `AprioriAlgorithm`, `PcaAlgorithm` and `QLearningAlgorithm` currently exist as direct algorithm classes and are not registered in `MachineLearningDispatcher`. This is infrastructure of classic deterministic/algorithmic ML methods, not a project-level `ILearningMachine`, not a product semantic reasoning model and not proof of trained user behavior.

## 1. Simple UML for draw.io

```mermaid
classDiagram
    class MachineLearningDispatcher {
      +Dispatch(string algorithmName, Dictionary request)
      +AvailableAlgorithms
    }
    class LinearRegressionService {
      +Train(string operationId, List inputs, List outputs)
      +Predict(double input)
    }
    class NaiveBayesService {
      +Train(string operationId, List docs, List labels)
      +Classify(string[] document)
    }
    class MultilayerPerceptronService {
      +Train(string operationId, List X, List Y, int hiddenSize, double learningRate, int epochs)
      +Predict(double[] input)
    }
    class KMeansService {
      +Cluster(string operationId, List points, int k, int maxIter)
      +Predict(double[] point)
    }
    class AlgorithmClasses {
      LinearRegressionAlgorithm
      NaiveBayesAlgorithm
      MultilayerPerceptronAlgorithm
      KMeansAlgorithm
      AprioriAlgorithm
      PcaAlgorithm
      QLearningAlgorithm
    }
    class Fabric {
      IModelStateStorage
      IPredictionLogger
      InMemoryModelStateStorage
      InMemoryPredictionLogger
    }
    class Tests {
      MachineLearningTests
    }

    MachineLearningDispatcher --> LinearRegressionService : "linear_regression"
    MachineLearningDispatcher --> NaiveBayesService : "naive_bayes"
    MachineLearningDispatcher --> MultilayerPerceptronService : "multilayer_perceptron"
    MachineLearningDispatcher --> KMeansService : "kmeans"
    LinearRegressionService --> AlgorithmClasses : "Fit/Predict"
    NaiveBayesService --> AlgorithmClasses : "Fit/Classify"
    MultilayerPerceptronService --> AlgorithmClasses : "Fit/Predict"
    KMeansService --> AlgorithmClasses : "Fit/Predict"
    LinearRegressionService --> Fabric : "Save/Log"
    NaiveBayesService --> Fabric : "Save/Log"
    MultilayerPerceptronService --> Fabric : "Save"
    KMeansService --> Fabric : "Save"
    Tests --> AlgorithmClasses : "unit coverage"
    Tests --> MachineLearningDispatcher : "dispatcher coverage"

    note for MachineLearningDispatcher "PARTIAL\nRoutes only the four service algorithms:\nlinear_regression, naive_bayes, multilayer_perceptron, kmeans."
    note for AlgorithmClasses "COMPLETE/PARTIAL BY CLASS\nContains 7 classic algorithms.\nApriori/PCA/Q-Learning have no service/dispatcher route."
    note for Fabric "COMPLETE FOR IN-MEMORY\nStores parameters and logs in process memory.\nThis is not a project asset lifecycle."
    note for Tests "UNIT/INTEGRATION EVIDENCE\nCovers the algorithms and the dispatcher,\nbut the tests were not run in this subphase."
```

| Node | What it does | Status | Where it lives |
|---|---|---|---|
| `MachineLearningDispatcher` | Returns a service by algorithm name | Partial: 4 of 7 algorithms | `14.MachineLearning/Dispatcher/MachineLearningDispatcher.cs` |
| `LinearRegressionService`, `NaiveBayesService`, `MultilayerPerceptronService`, `KMeansService` | Wrap the algorithms, storage/logging/idempotency | Implemented for their algorithms | `14.MachineLearning/Algorithms/*.cs` |
| `AlgorithmClasses` | The real computations: OLS, Naive Bayes, SGD backprop, K-Means, Apriori, PCA, Q-Learning | Implemented as local algorithms | `14.MachineLearning/Algorithms/*.cs` |
| `Fabric` | In-memory storage/logger/data provider contracts | Implemented for test/in-memory infrastructure | `14.MachineLearning/Fabric/*.cs` |
| `Tests` | Check the behavior of the algorithms and infrastructure | There is source evidence; not run here | `Tests/MachineLearningTests.cs` |

### 1.1 Simulations for each UML connection

| Connection | Example input | What happens | Expected result | What must not be taken as a result |
|---|---|---|---|---|
| `MachineLearningDispatcher -> LinearRegressionService` | `algorithmName="linear_regression"` | The dispatcher looks up the key in `_services` | Returns the service object in `OperationResult<object>.Ok` | This must not be treated as model training or a check of regression quality |
| `MachineLearningDispatcher -> unknown` | `algorithmName="pca"` | There is no `pca` key in `_services` | `OperationResult<object>.Fail("Unknown algorithm: pca")` | One must not write that PCA is available via the dispatcher |
| `LinearRegressionService -> AlgorithmClasses` | `Train("op1", [1,2,3], [2,4,6])` | The service checks idempotency, calls `Fit`, saves slope/intercept | Returns `{"slope": 2, "intercept": 0}` approximately | In-memory save must not be treated as a full model server/checkpoint lifecycle |
| `NaiveBayesService -> Fabric` | `Classify(["good","great"])` after train | The algorithm chooses a class, the service writes a prediction log | Returns `"positive"` and adds a log entry | The log does not prove semantic reasoning beyond the training tokens |
| `KMeansService -> AlgorithmClasses` | points of two clusters, `k=2` | K-Means++ chooses centroids, iteratively assigns points | the labels separate the two groups | Clustering must not be presented as a trainable product model |
| `Tests -> AlgorithmClasses` | `MachineLearningTests.MLP_LearnXOR` | The test sets XOR and checks the output bounds | After training `[0,0]` is close to 0, `[0,1]` is close to 1 | This test does not prove MLP quality on product tasks |

## 2. No-code algorithms, in the operator's language

### 2.1 Linear regression

Analogy: there is a ruler with which we draw a straight line through a cloud of points so that the average error is minimal.

| Field | Value |
|---|---|
| Class | `LinearRegressionAlgorithm` |
| Method | `Fit(List<double> inputs, List<double> outputs)` |
| Input | `inputs=[1,2,3]`, `outputs=[2,4,6]` |
| Output | `OperationResult<Dictionary<string,double>>` with `slope`, `intercept` |
| Status | Implemented deterministic classic ML algorithm |

Steps: checks that there are at least two points; checks equal length of inputs/outputs; computes the means of `x` and `y`; computes the numerator and denominator of the ordinary least squares formula; if all `x` are identical, returns fail; otherwise saves `_slope`, `_intercept`, `_fitted=true`. `Predict(double input)` after fit returns `_slope * input + _intercept`, before fit returns fail.

Branches: too few points -> fail; different list lengths -> fail; all `x` identical -> fail; a normal set -> the line's parameters.

What it does NOT do: does not train a neural network, does not create a checkpoint/manifest, does not prove the product's ability to reason.

### 2.2 Naive Bayes

Analogy: a mail sorter counts which words appeared more often in each folder, and chooses the folder with the largest sum of log probabilities.

| Field | Value |
|---|---|
| Class | `NaiveBayesAlgorithm` |
| Method | `Fit(List<string[]> documents, List<string> labels)` and `Classify(string[] document)` |
| Input | documents like `[["good","great"], ["bad","awful"]]`, labels `["positive","negative"]` |
| Output | `OperationResult<bool>` for fit, `OperationResult<string>` for the class |
| Status | Implemented multinomial Naive Bayes with Laplace smoothing |

Steps: checks the presence of documents and a match in the number of labels; builds a list of classes; for each class computes the log prior; collects a vocabulary over all documents; for each word computes the log probability with Laplace smoothing; on classification sums the prior and the known word probabilities for each class; chooses the class with the maximum score.

Branches: an empty train set -> fail; a mismatch of docs/labels -> fail; classify before fit -> fail; an empty document -> fail; unknown words simply do not add score.

What it does NOT do: does not understand the meaning of the text; this is probabilistic classification by tokens, not a semantic authority.

### 2.3 Multilayer Perceptron

Analogy: a small adjustable network of knobs: the input moves the hidden knobs, the hidden moves the output, the error goes back and tweaks the weights.

| Field | Value |
|---|---|
| Class | `MultilayerPerceptronAlgorithm` |
| Method | `Fit(List<double[]> X, List<double[]> Y, int hiddenSize=8, double learningRate=0.1, int epochs=500)` |
| Input | `X=[[0,0],[0,1]]`, `Y=[[0],[1]]` |
| Output | `OperationResult<bool>`; `Predict(double[] input)` returns `OperationResult<double[]>` |
| Status | Implemented toy neural algorithm, but not a project asset model |

Steps: checks a non-empty training set; initializes the weights with seed=42; for each epoch and each sample does forward through a sigmoid hidden and a sigmoid output; computes the output error; computes the hidden error; updates `_w2/_b2`, then `_w1/_b1`; after fit sets `_fitted=true`. `Predict` repeats forward without training.

Branches: an empty train set -> fail; predict before fit -> fail; a normal train -> changes the in-memory weights.

What it does NOT do: does not save the weights as a portable checkpoint, is not connected to `ILearningMachine`, does not prove quality on real product data.

### 2.4 K-Means

Analogy: we lay out points near the nearest flags, then move each flag to the center of its group, until the layout stops changing.

| Field | Value |
|---|---|
| Class | `KMeansAlgorithm` |
| Method | `Fit(List<double[]> points, int k, int maxIterations=100)` |
| Input | `points=[[0,0],[1,0],[10,10]]`, `k=2` |
| Output | `OperationResult<int[]>` with a cluster id for each point |
| Status | Implemented deterministic seeded clustering algorithm |

Steps: checks non-empty points and a valid `k`; chooses K-Means++ centroids with `Random(42)`; in a loop assigns each point to the nearest centroid; if the assignments did not change, exits; otherwise recomputes the centroids by the mean of the cluster members; `Predict` after fit chooses the nearest centroid.

Branches: no points -> fail; `k<=0` or `k>points.Count` -> fail; an empty cluster during recomputation is skipped; predict before fit -> fail.

What it does NOT do: is not semantic retrieval or a trained reasoning model; a cluster id by itself does not explain the meaning of a group.

### 2.5 Apriori

Analogy: a store looks for goods that are often bought together, first by a single good, then in pairs, then in triples, and builds rules of the form "if A, then often B".

| Field | Value |
|---|---|
| Class | `AprioriAlgorithm` |
| Method | `Mine(List<List<string>> transactions, double minimumSupport=0.5, double minimumConfidence=0.7)` |
| Input | `transactions=[["milk","bread"],["milk","butter"]]` |
| Output | `OperationResult<(List<FrequentItemset>, List<AssociationRule>)>` |
| Status | Implemented deterministic association mining |

Steps: checks non-empty transactions; computes frequent 1-itemsets by support; a level-wise join generates larger candidates; computes the support of each candidate; saves the frequent sets; for frequent itemsets of size 2+ iterates over subsets as the consequent, builds the antecedent, computes confidence and adds a rule if the confidence is sufficient.

Branches: an empty list of transactions -> fail; a candidate below minimumSupport is discarded; an antecedent without known support is skipped; confidence below the threshold does not become a rule.

What it does NOT do: does not train a model of meanings; association rules are not equal to causality.

### 2.6 PCA

Analogy: we rotate the ruler of data so that the first new axis looks toward where the spread of points is the largest, then take the next axes.

| Field | Value |
|---|---|
| Class | `PcaAlgorithm` |
| Method | `Fit(List<double[]> data, int numComponents)` and `Transform(double[] point)` |
| Input | `data` of 4D vectors, `numComponents=2` |
| Output | `OperationResult<bool>` for fit, `OperationResult<double[]>` for the projection |
| Status | Implemented PCA via covariance + power iteration deflation |

Steps: checks non-empty data; checks `numComponents <= dim`; computes the mean; centers the data; builds the covariance matrix; for each component runs power iteration; saves the eigenvalue and vector; deflates the covariance; `Transform` centers the point and projects it onto the components.

Branches: an empty dataset -> fail; too many components -> fail; transform before fit -> fail; a zero norm in the helper does not normalize the vector further.

What it does NOT do: does not classify and does not explain the subject area; dimensionality reduction is not proof of a trained model.

### 2.7 Q-Learning

Analogy: an agent walks over a small table of states and actions, tries random actions for exploration, and then reinforces the actions that gave a reward.

| Field | Value |
|---|---|
| Class | `QLearningAlgorithm` |
| Method | `Train(Func<int,int,(int nextState,double reward,bool done)> environment, int numStates, int numActions, int episodes=1000, double learningRate=0.1, double discountFactor=0.9, double explorationRate=0.3)` |
| Input | environment callback, `numStates=2`, `numActions=2` |
| Output | `OperationResult<bool>`; `BestAction(int state)` -> `OperationResult<int>` |
| Status | Implemented tabular reinforcement learning helper |

Steps: checks positive numStates/numActions; initializes the Q-table with zeros; for each episode chooses a random start; up to 200 steps chooses an action epsilon-greedy; calls the environment; computes the max future Q; updates the Q-value by the Bellman formula; stops the episode on `done`; `BestAction` takes the action with the maximum Q.

Branches: invalid states/actions -> fail; the exploration branch chooses a random action; the exploitation branch chooses the best action; `done=true` ends the episode; `BestAction` before train -> fail.

What it does NOT do: does not control a real environment without a callback, does not save a policy asset, does not prove product autonomy.

### 2.8 Dispatcher and Fabric

`MachineLearningDispatcher.Dispatch(string algorithmName, Dictionary<string, object> request)` currently does not execute train/predict itself: it returns a service object for four known names. `IModelStateStorage`, `IPredictionLogger`, `ITrainingDataProvider` define the storage/data/logging boundaries; the `InMemory*` implementations store everything in process memory.

What it does NOT do: the dispatcher does not cover Apriori/PCA/Q-Learning, is not a plugin-facing API and does not turn in-memory parameters into a project model asset.

## 3. Where the logic sits in the code

| Logic | File | Why this location matters | What it proves | What it does not prove |
|---|---|---|---|---|
| OLS regression | `14.MachineLearning/Algorithms/LinearRegression.cs:11` | This is where slope/intercept are computed | There is a current fit/predict implementation | No asset lifecycle |
| Naive Bayes | `14.MachineLearning/Algorithms/NaiveBayes.cs:12` | This is where priors/probabilities are built | There is a token-based classifier | No semantic reasoning |
| MLP | `14.MachineLearning/Algorithms/MultilayerPerceptron.cs:14` | This is the forward/backprop loop | There are trainable in-memory weights | No checkpoint/manifest |
| K-Means | `14.MachineLearning/Algorithms/KMeans.cs:10` | This is the clustering loop | There is clustering and predict | No semantic interpretation of clusters |
| Apriori | `14.MachineLearning/Algorithms/Apriori.cs:15` | This is frequent itemsets + rules | There is association mining | No causality/semantic model |
| PCA | `14.MachineLearning/Algorithms/PCA.cs:13` | This is covariance + power iteration | There is dimensionality reduction | No classifier/trained product model |
| Q-Learning | `14.MachineLearning/Algorithms/QLearning.cs:11` | This is the Q-table training loop | There is a tabular RL helper | No real environment/product policy |
| Dispatcher | `14.MachineLearning/Dispatcher/MachineLearningDispatcher.cs:15` | This is the list of available service routes | 4 registered services are visible | No routes for Apriori/PCA/Q-Learning |
| In-memory storage/logger | `14.MachineLearning/Fabric/InMemoryStorage.cs:3` | This is where parameters and prediction logs are saved | There is test infrastructure | Not a model server and not a durable checkpoint |

## 4. Short important code quotations

> **Why it matters:** this branch shows an honest protection of linear regression from a mathematically impossible slope: if all `x` are identical, the denominator is nearly zero.
>
> **Input:** `inputs=[2,2,2]`, `outputs=[1,2,3]`.
>
> **Output:** a fail, not a fake slope.
>
> **What it does not prove:** model quality on real data.

```csharp
// 14.MachineLearning/Algorithms/LinearRegression.cs:32-35
if (Math.Abs(denominator) < 1e-12)
    return OperationResult<Dictionary<string, double>>.Fail("All inputs identical; slope undefined");

_slope     = numerator / denominator;
```

> **Why it matters:** Naive Bayes uses Laplace smoothing so that a word with a zero count does not zero out the whole class score.
>
> **Input:** a word absent from the class but present in the vocabulary.
>
> **Output:** a small log probability instead of a zero-probability failure.
>
> **What it does not prove:** understanding of the text or transferability to new domains.

```csharp
// 14.MachineLearning/Algorithms/NaiveBayes.cs:48-53
foreach (var word in vocab)
{
    int count = wordCounts.GetValueOrDefault(word);
    // Laplace smoothing: (count + 1) / (total + vocabSize)
    _wordProbs[cls][word] = Math.Log((count + 1.0) / (totalWords + vocab.Count));
}
```

> **Why it matters:** the MLP really changes the weights via backprop, rather than returning a fixed answer.
>
> **Input:** one training sample `X[s]` and the expected `Y[s]`.
>
> **Output:** updated `_w2/_b2` and then `_w1/_b1`.
>
> **What it does not prove:** that the weights are saved as a project checkpoint or that this is an `ILearningMachine`.

```csharp
// 14.MachineLearning/Algorithms/MultilayerPerceptron.cs:53-64
for (int o = 0; o < outputSize; o++)
{
    for (int h = 0; h < hiddenSize; h++)
        _w2[o, h] += learningRate * outErr[o] * hidden[h];
    _b2[o] += learningRate * outErr[o];
}
for (int h = 0; h < hiddenSize; h++)
{
    for (int i = 0; i < inputSize; i++)
        _w1[h, i] += learningRate * hidErr[h] * X[s][i];
    _b1[h] += learningRate * hidErr[h];
}
```

> **Why it matters:** K-Means has an explicit fail for an incorrect `k`, so it does not pretend that clustering is possible when `k > points.Count`.
>
> **Input:** one point and `k=5`.
>
> **Output:** `OperationResult<int[]>.Fail`.
>
> **What it does not prove:** cluster quality with a valid `k`.

```csharp
// 14.MachineLearning/Algorithms/KMeans.cs:10-14
public OperationResult<int[]> Fit(List<double[]> points, int k, int maxIterations = 100)
{
    if (points.Count == 0) return OperationResult<int[]>.Fail("No data points");
    if (k <= 0 || k > points.Count)
        return OperationResult<int[]>.Fail($"k={k} invalid for {points.Count} points");
```

> **Why it matters:** the dispatcher registration is honestly limited to four service names.
>
> **Input:** `"linear_regression"` works; `"pca"` will not be found.
>
> **Output:** a service object or a fail unknown algorithm.
>
> **What it does not prove:** that all seven algorithm classes are available via the dispatcher.

```csharp
// 14.MachineLearning/Dispatcher/MachineLearningDispatcher.cs:15-22
_services = new()
{
    ["linear_regression"]     = linearRegression,
    ["naive_bayes"]           = naiveBayes,
    ["multilayer_perceptron"] = multilayerPerceptron,
    ["kmeans"]                = kMeans
};
```

> **Why it matters:** storage here is in-memory: it copies the dictionary into a local `_store`, but does not write a durable artifact.
>
> **Input:** `Save("linear_regression", {"slope": 2.0})`.
>
> **Output:** the value can be loaded from the same storage object.
>
> **What it does not prove:** a model repository, manifest, import/export or fresh-load between processes.

```csharp
// 14.MachineLearning/Fabric/InMemoryStorage.cs:7-10
public OperationResult<bool> Save(string name, Dictionary<string, object> p)
{
    _store[name] = new Dictionary<string, object>(p);
    return OperationResult<bool>.Ok(true);
}
```

## 5. Test, e2e and QA mapping

The tests were not run in this documentation subphase.

| Behavior | Verification level | Current evidence | Status | Next work order if gap |
|---|---|---|---|---|
| `OperationResult.Ok/Fail` | unit | `Tests/MachineLearningTests.cs` contains `OperationResult_Ok_HasValue`, `OperationResult_Fail_HasError` | source-mapped, not run here | Run a targeted test in a separate authorized test phase |
| `LinearRegressionAlgorithm` fit/predict/fail/idempotency | unit/integration | tests for slope/intercept, predict, too few points, predict-before-fit, service idempotency | source-mapped, not run here | Targeted test run if authorized |
| `NaiveBayesAlgorithm` train/classify/fail | unit | tests for positive/negative, empty train, classify before fit | source-mapped, not run here | Targeted test run if authorized |
| `MultilayerPerceptronAlgorithm` XOR/backprop/fail | unit | tests for XOR, predict-before-fit, empty train set | source-mapped, not run here | Runtime metrics beyond XOR need future QA |
| `KMeansAlgorithm` clustering/predict/fail | unit | tests for two clusters, centroids count, invalid k, predict cluster range | source-mapped, not run here | Product clustering quality not covered |
| `AprioriAlgorithm` itemsets/rules/fail | unit | tests for frequent itemsets, rules, empty transactions | source-mapped, not run here | No causality/product-quality evidence |
| `PcaAlgorithm` projection/variance/fail | unit | tests for dimensionality reduction, variance ratio, too many components | source-mapped, not run here | No downstream model-quality evidence |
| `QLearningAlgorithm` Q-table/best action/fail | unit | tests for simple environment, Q update, invalid states | source-mapped, not run here | No real environment/e2e policy evidence |
| `MachineLearningDispatcher` | integration | tests for known/unknown/list algorithms | source-mapped, not run here | Does not cover Apriori/PCA/Q-Learning dispatcher routes because they do not exist |
| `InMemoryModelStateStorage` | unit | tests for save/load and missing load | source-mapped, not run here | Not durable asset lifecycle |

## 6. Anti-false-done checks

| Check | Honest conclusion for `14.MachineLearning` |
|---|---|
| No JSON-only done | `14.MachineLearning/state.json` says `complete`, but this is not runtime proof and does not replace source/test evidence. |
| No hidden deterministic shortcut | OLS, Naive Bayes, K-Means, Apriori, PCA and the Q-table are described as classic algorithms/infrastructure, not as a semantic trainable authority. |
| No class-name-only proof | The presence of the classes `AprioriAlgorithm`, `PcaAlgorithm`, `QLearningAlgorithm` does not mean they are registered in the dispatcher or available as a project-level service. |
| No trained product claim | MLP and Q-Learning can change in-memory weights/Q-table, but without checkpoint/manifest/locator/fresh-load/continue-training evidence this is not a finished product model. |
| No smoke/static substitution | The source refs and tests mapping show narrow evidence; a full build/test/runtime was not performed here and is not claimed. |

## 7. Component status

| Component | File | Status | What actually works now |
|---|---|---|---|
| `OperationResult<T>` | `14.MachineLearning/Core/Core.cs` | Complete for the local project | `Ok`, `Fail`, `MapError` envelope for the algorithms |
| `LinearRegressionAlgorithm` | `14.MachineLearning/Algorithms/LinearRegression.cs` | Complete as a classic algorithm | `Fit`, `Predict`, `LoadParameters`, fail branches |
| `LinearRegressionService` | `14.MachineLearning/Algorithms/LinearRegression.cs` | Partial service infrastructure | `Train` saves parameters in-memory, `Predict` logs; no durable asset lifecycle |
| `NaiveBayesAlgorithm` | `14.MachineLearning/Algorithms/NaiveBayes.cs` | Complete as a classic algorithm | `Fit` builds priors/probs, `Classify` chooses a class |
| `NaiveBayesService` | `14.MachineLearning/Algorithms/NaiveBayes.cs` | Partial service infrastructure | `Train` saves a fitted marker, `Classify` logs |
| `MultilayerPerceptronAlgorithm` | `14.MachineLearning/Algorithms/MultilayerPerceptron.cs` | Complete as a toy neural algorithm | `Fit` backprop, `Predict` forward |
| `MultilayerPerceptronService` | `14.MachineLearning/Algorithms/MultilayerPerceptron.cs` | Partial service infrastructure | idempotent train + in-memory fitted marker; the weights are not exported |
| `KMeansAlgorithm` | `14.MachineLearning/Algorithms/KMeans.cs` | Complete as a clustering algorithm | `Fit`, `Predict`, `Centroids` |
| `KMeansService` | `14.MachineLearning/Algorithms/KMeans.cs` | Partial service infrastructure | `Cluster` saves `k`, `Predict` delegates to the algorithm |
| `AprioriAlgorithm` | `14.MachineLearning/Algorithms/Apriori.cs` | Complete as a direct algorithm | `Mine` returns frequent itemsets and association rules; no service route |
| `PcaAlgorithm` | `14.MachineLearning/Algorithms/PCA.cs` | Complete as a direct algorithm | `Fit`, `Transform`, `ExplainedVarianceRatio`; no service route |
| `QLearningAlgorithm` | `14.MachineLearning/Algorithms/QLearning.cs` | Complete as a direct algorithm | `Train`, `BestAction`, `QValue`; no service route |
| `MachineLearningDispatcher` | `14.MachineLearning/Dispatcher/MachineLearningDispatcher.cs` | Partial | Registers only `linear_regression`, `naive_bayes`, `multilayer_perceptron`, `kmeans` |
| `InMemoryModelStateStorage` | `14.MachineLearning/Fabric/InMemoryStorage.cs` | Complete for in-memory tests | `Save`, `Load`, `List`; not durable persistence |
| `InMemoryPredictionLogger` | `14.MachineLearning/Fabric/InMemoryStorage.cs` | Complete for in-memory tests | `Log` adds entries to a list |

## 8. `ILearningMachine` hierarchy

In the files read of `14.MachineLearning` there is no implementation of `ILearningMachine`, `ILogicalOrchestrator`, `ILogicalFlow`, `ILogicalUnit` or `ILearningMachinePair`. Therefore this project must not be described as a five-level trainable product hierarchy. Its current boundary is classic algorithm/service/fabric classes inside the `MachineLearning` namespace.
