# Machine Learning Algorithms — Sub-Projects 14 through 20
## Solution: LlmFromScratch.sln — Implementation Plan v1.0

**Project:** llm-mvp-core  
**Language:** C# / .NET 8  
**External packages:** none except TorchSharp for matrix operations (already used in sub-projects 03–04)  
**Document purpose:** Step-by-step implementation plan for seven classic machine learning algorithms as independent sub-projects within the same solution  
**This document does not touch XIIGen work.**

**What this document adds to the solution:**

| Sub-project number | Algorithm name |
|---|---|
| 14 | Linear Regression (Линейная регрессия — предсказание числа по входным данным) |
| 15 | Naive Bayes Classifier (Наивный байесовский классификатор — классификация текстов по вероятностям) |
| 16 | Multilayer Perceptron Neural Network (Многослойный перцептрон — нейронная сеть с обучением обратным распространением ошибки) |
| 17 | K-Means Clustering (Кластеризация методом K-средних — разбиение данных на группы без учителя) |
| 18 | Apriori Association Rules (Алгоритм Апriori — поиск связей между товарами или событиями) |
| 19 | Principal Component Analysis (Метод главных компонент — сжатие размерности данных) |
| 20 | Q-Learning Reinforcement (Обучение с подкреплением методом Q-таблиц — агент учится на наградах и штрафах) |

**Reference source for all seven algorithms:**  
`ml_algorithms_implementations_en.md` — Python reference implementations, available alongside this plan. Every algorithm in this plan is a direct C# translation of the Python version, with identical mathematical logic and the same test cases used to verify output.

---

## Part 1 — Coding Rules (Genie DNA) Applied to All Seven Sub-Projects

All seven sub-projects follow the same nine coding rules established in the main implementation plan (v2.0). This section restates how each rule applies in the context of machine learning algorithms, so there is no ambiguity.

### Coding rule 1 — No typed model classes (data travels as dictionaries)

Training data, prediction results, and model parameters are stored in `Dictionary<string, object>` or simple `record` types with only primitive fields (numbers, strings, lists of numbers). No class hierarchies. No model classes with business logic inside.

Correct:
```csharp
// TrainingData is a plain dictionary — it carries numbers and nothing else
var trainingData = new Dictionary<string, object>
{
    // "inputs" holds the list of input feature vectors
    ["inputs"] = new List<double[]> { new[] { 1.0, 2.0 }, new[] { 3.0, 4.0 } },
    // "labels" holds the expected output for each input
    ["labels"] = new List<double> { 0.5, 1.5 }
};
```

Not correct:
```csharp
// WRONG — this is a typed model class, it violates coding rule 1
class TrainingData
{
    public List<double[]> Inputs { get; set; }
    public List<double> Labels { get; set; }
}
```

### Coding rule 2 — No hardcoded filter or search objects inside business methods

When building a query or a filter over training data, the filter parameters are assembled by a dedicated `BuildTrainingFilter(config)` method, not written inline inside the training loop.

### Coding rule 3 — All public methods return OperationResult<T>

Every method that can fail (due to bad input, numerical instability, empty data) returns `OperationResult<T>`. The `OperationResult<T>` type is defined in `Core/OperationResult.cs` (already established in sub-project 13, Code Optimizer). All seven sub-projects reuse that same definition — no copies.

### Coding rule 4 — All service classes extend BaseService

`LinearRegressionService`, `NaiveBayesService`, `NeuralNetworkService`, etc. all extend `BaseService` from `Core/BaseService.cs`.

### Coding rule 5 — Scope comes from context, never from a parameter

If any training or prediction operation needs to know which user or session it belongs to, that information is read from `IOperationContext`. No method has a `userId`, `sessionId`, or `tenantId` parameter.

### Coding rule 6 — No entity-specific API handlers

All external calls go through the single `MachineLearningDispatcher` (defined at the end of this plan), not through seven separate controllers.

### Coding rule 7 — Idempotency on training operations

Every `TrainAsync` method accepts an `operationId`. If the same `operationId` is submitted twice (e.g. after a network timeout), the second call returns the saved result without retraining.

### Coding rule 8 — Save model weights before dispatching "training complete" event

After training finishes, the trained model state is persisted (via `IModelStateStorage`) before any "training complete" notification is dispatched.

### Coding rule 9 — Wrap all notifications in StandardEvent

Any event dispatched between components uses `StandardEvent` from `Core/StandardEvent.cs`.

---

## Part 2 — Shared Fabric Interfaces for All Seven Sub-Projects

These interfaces are defined once in `14.MachineLearning/Fabric/` and used by all seven algorithm implementations.

```csharp
// IModelStateStorage hides where trained model parameters are saved and loaded from.
// An algorithm service never reads or writes files directly — it always uses this interface.
// Concrete implementations: InMemoryModelStateStorage (tests), FileSystemModelStateStorage (production)
public interface IModelStateStorage
{
    // Save the trained parameters of an algorithm under a given name
    // Parameters are stored as a plain dictionary of numbers, lists, and strings
    Task<OperationResult<bool>> SaveAsync(string modelName, Dictionary<string, object> parameters);

    // Load previously saved parameters by name
    Task<OperationResult<Dictionary<string, object>>> LoadAsync(string modelName);

    // Return the names of all models currently in storage
    Task<OperationResult<List<string>>> ListAsync();
}

// ITrainingDataProvider hides where training examples come from.
// The algorithm service does not know whether data comes from a CSV file,
// from memory, from a database, or from a test fixture.
// Concrete implementations: InMemoryTrainingDataProvider, CsvTrainingDataProvider
public interface ITrainingDataProvider
{
    // Return training examples as a list of feature vectors and a list of expected labels
    // featureVectors — each row is one example; each column is one measured property
    // labels — the correct answer for each row (a number for regression, a string for classification)
    Task<OperationResult<(List<double[]> FeatureVectors, List<object> Labels)>> LoadAsync(string datasetName);
}

// IPredictionLogger records every prediction made so results can be reviewed and audited.
// Concrete implementations: InMemoryPredictionLogger, ConsolePredictionLogger
public interface IPredictionLogger
{
    // Record one prediction: what was the input, what was the output, and when did it happen
    Task<OperationResult<bool>> LogAsync(string algorithmName, Dictionary<string, object> input, Dictionary<string, object> output);
}
```

---

## Part 3 — Shared Folder Structure

All seven algorithm implementations live under one project folder to keep the solution tidy:

```
14.MachineLearning/
├── Fabric/
│   ├── IModelStateStorage.cs       — interface: save and load trained model parameters
│   ├── ITrainingDataProvider.cs    — interface: supply training examples to the algorithm
│   ├── IPredictionLogger.cs        — interface: record every prediction for auditing
│   ├── FileSystemModelStateStorage.cs  — saves model parameters as JSON files on disk
│   ├── InMemoryModelStateStorage.cs    — keeps model parameters in memory (used in tests)
│   ├── CsvTrainingDataProvider.cs      — reads training data from CSV files
│   └── InMemoryTrainingDataProvider.cs — supplies training data from in-memory lists (tests)
├── Core/
│   ├── BaseService.cs              — every algorithm service extends this
│   ├── OperationResult.cs          — result container (success or failure) for all public methods
│   ├── StandardEvent.cs            — envelope wrapper for all dispatched notifications
│   └── IdempotencyStore.cs         — checks and records whether an operationId was already processed
├── Algorithms/
│   ├── 01-LinearRegression/        — sub-project 14
│   ├── 02-NaiveBayes/              — sub-project 15
│   ├── 03-NeuralNetwork/           — sub-project 16
│   ├── 04-KMeans/                  — sub-project 17
│   ├── 05-Apriori/                 — sub-project 18
│   ├── 06-PCA/                     — sub-project 19
│   └── 07-QLearning/               — sub-project 20
├── Dispatcher/
│   └── MachineLearningDispatcher.cs — routes all external requests to the right algorithm
└── Tests/
    ├── LinearRegressionTests.cs
    ├── NaiveBayesTests.cs
    ├── NeuralNetworkTests.cs
    ├── KMeansTests.cs
    ├── AprioriTests.cs
    ├── PcaTests.cs
    └── QLearningTests.cs
```

---

## Part 4 — Abbreviation Table for This Document

| Short form used in code | Full name | Plain-language explanation |
|---|---|---|
| Linear Regression | Linear Regression (Линейная регрессия) | finding the best straight line through a set of data points to predict new values |
| Naive Bayes (NB) | Naive Bayes Classifier (Наивный байесовский классификатор) | classifies text or data by calculating the probability that it belongs to each known category, assuming all features are independent of each other |
| Multilayer Perceptron (MLP) | Multilayer Perceptron Neural Network (Многослойный перцептрон) | a neural network with at least one hidden layer between input and output, trained by backward error propagation |
| Backpropagation | Backward Error Propagation (Обратное распространение ошибки) | the algorithm that adjusts weights in a neural network by tracing how much each weight contributed to the prediction error |
| Sigmoid | Sigmoid Activation Function (Сигмоидная функция активации) | a mathematical function that squashes any number into the range 0 to 1; used to produce probability-like outputs |
| K-Means | K-Means Clustering (Кластеризация методом K-средних) | groups data points by repeatedly assigning each point to the nearest cluster center and recalculating the center |
| Centroid | Cluster Center (Центр кластера) | the average position of all points currently assigned to a cluster |
| Apriori | Apriori Association Rules Algorithm (Алгоритм поиска ассоциативных правил) | discovers which items tend to appear together by counting combinations in a transaction database |
| Support | Rule Support (Поддержка правила) | the fraction of all transactions that contain a specific combination of items |
| Confidence | Rule Confidence (Уверенность правила) | given that one item was bought, the probability that another specific item was also bought |
| Principal Component Analysis (PCA) | Principal Component Analysis (Метод главных компонент) | finds the directions of greatest variance in high-dimensional data and projects the data onto fewer dimensions |
| Eigenvector | Principal Direction (Главное направление / собственный вектор) | a direction in the data space along which the data has maximum spread; computed from the covariance matrix |
| Eigenvalue | Variance Explained (Объяснённая дисперсия / собственное значение) | a number measuring how much of the total data spread is captured by the corresponding principal direction |
| Covariance Matrix | Covariance Matrix (Матрица ковариации) | a square matrix where each entry measures how much two data dimensions vary together |
| Q-Learning | Q-Learning Reinforcement Algorithm (Алгоритм обучения с подкреплением методом Q-таблиц) | an agent learns to navigate an environment by trying actions and recording which state-action pairs lead to rewards |
| Q-table | Action Value Table (Таблица ценностей действий) | a lookup table mapping every (state, action) pair to the expected total reward from that point onward |
| Epsilon-greedy | Exploration-Exploitation Balance (Баланс исследования и использования знаний) | with probability epsilon the agent tries a random action (explores); otherwise it picks the best known action (exploits) |
| Learning rate | Learning Rate (Скорость обучения) | a small number (e.g. 0.01) that controls how much the model adjusts its parameters in response to each new piece of training data |
| Discount factor | Discount Factor (Коэффициент дисконтирования будущих наград) | a number between 0 and 1 that makes future rewards worth less than immediate rewards |

In all source code files, variable names must spell out the concept in full:
- `lr` → `learningRate`
- `gamma` → `discountFactor`
- `eps` → `explorationProbability`
- `pca` → `principalComponentAnalysis` or `pcaTransformer`
- `nb` → `naiveBayesClassifier`
- `mlp` → `multilayerPerceptron`

---

## Sub-Project 14 — Linear Regression (Линейная регрессия)

**What this algorithm does in plain language:** Given a table of measurements (for example: apartment area → price), the algorithm finds the straight line that passes as close as possible to all the data points. The line is defined by two numbers: the slope (how much the output grows per unit of input) and the intercept (the output value when the input is zero). Once the line is found, it can predict the output for any new input.

**Reference source:** `ml_algorithms_implementations_en.md` — Section "1. Linear Regression", Python implementation, ~50 lines.

**Dependencies within the solution:** None. Sub-project 14 is self-contained.

**Folder:** `14.MachineLearning/Algorithms/01-LinearRegression/`

### Fabric interface additions for this sub-project

No new fabric interface is needed. Uses `IModelStateStorage`, `ITrainingDataProvider`, and `IPredictionLogger` from the shared fabric layer.

### Phase 1 — Core algorithm (1.5 days)

What to write: `LinearRegressionAlgorithm.cs`

```csharp
// LinearRegressionAlgorithm contains the pure mathematical logic of linear regression.
// It does not talk to files, databases, or loggers.
// It only knows how to compute the slope and intercept from numbers.
public class LinearRegressionAlgorithm
{
    // The slope of the fitted line — how much the output changes when the input increases by one unit
    // Named "a" in the formula Y = a × X + b
    private double slope;

    // The intercept of the fitted line — the output value when the input equals zero
    // Named "b" in the formula Y = a × X + b
    private double intercept;

    // Train the algorithm: find the slope and intercept that minimize prediction error
    // inputValues — a list of input measurements (e.g. apartment areas in square meters)
    // outputValues — the corresponding known answers (e.g. prices in hundreds of thousands)
    // Returns the fitted slope and intercept as a dictionary so they can be saved
    public OperationResult<Dictionary<string, double>> Fit(
        List<double> inputValues,
        List<double> outputValues)
    {
        // Verify that we have at least two data points — one point alone defines infinitely many lines
        if (inputValues.Count < 2)
        {
            // Return a failure result explaining what went wrong — do not throw
            return OperationResult<Dictionary<string, double>>.Fail(
                "Training requires at least two data points to fit a line.",
                "TOO_FEW_POINTS");
        }

        // Verify that inputs and outputs have the same count — each input must have a corresponding output
        if (inputValues.Count != outputValues.Count)
        {
            return OperationResult<Dictionary<string, double>>.Fail(
                $"Input count ({inputValues.Count}) does not match output count ({outputValues.Count}).",
                "SIZE_MISMATCH");
        }

        // Count how many data points we have
        int pointCount = inputValues.Count;

        // Calculate the average (mean) of all input values
        // The mean is the sum of all values divided by the count
        double meanInput = inputValues.Sum() / pointCount;

        // Calculate the average (mean) of all output values
        double meanOutput = outputValues.Sum() / pointCount;

        // Accumulate the numerator of the slope formula:
        // sum of (each input minus mean input) times (each output minus mean output)
        double slopeNumerator = 0;

        // Accumulate the denominator of the slope formula:
        // sum of (each input minus mean input) squared
        double slopeDenominator = 0;

        // Walk through every data point and accumulate both sums
        for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
        {
            // How far is this input from the average input?
            double inputDeviation = inputValues[pointIndex] - meanInput;

            // How far is this output from the average output?
            double outputDeviation = outputValues[pointIndex] - meanOutput;

            // Add the product of both deviations to the numerator
            slopeNumerator += inputDeviation * outputDeviation;

            // Add the squared input deviation to the denominator
            slopeDenominator += inputDeviation * inputDeviation;
        }

        // Guard against the case where all inputs are identical — division by zero would occur
        if (Math.Abs(slopeDenominator) < 1e-12)
        {
            return OperationResult<Dictionary<string, double>>.Fail(
                "All input values are identical — cannot fit a line through a single vertical column of points.",
                "ZERO_VARIANCE_INPUT");
        }

        // Compute the slope: change in output per unit change in input
        slope = slopeNumerator / slopeDenominator;

        // Compute the intercept: adjust so the line passes through the mean point
        intercept = meanOutput - slope * meanInput;

        // Return the fitted parameters as a dictionary so they can be stored via IModelStateStorage
        return OperationResult<Dictionary<string, double>>.Ok(
            new Dictionary<string, double>
            {
                // Store the slope under a clear key
                ["slope"] = slope,
                // Store the intercept under a clear key
                ["intercept"] = intercept
            });
    }

    // Predict the output for a single new input value using the fitted line
    // newInput — the input for which we want to predict the output
    public OperationResult<double> Predict(double newInput)
    {
        // Apply the line formula: Y = slope × X + intercept
        double prediction = slope * newInput + intercept;

        // Return the prediction wrapped in a success result
        return OperationResult<double>.Ok(prediction);
    }

    // Load previously saved parameters back into this instance
    // Allows using a saved model without retraining
    public void LoadParameters(Dictionary<string, object> savedParameters)
    {
        // Read the slope from the dictionary and cast it to a floating-point number
        slope = Convert.ToDouble(savedParameters["slope"]);

        // Read the intercept from the dictionary and cast it to a floating-point number
        intercept = Convert.ToDouble(savedParameters["intercept"]);
    }
}
```

Phase exit criterion: `dotnet build` exits 0, no warnings.

### Phase 2 — Service wrapper with fabric and coding rules (1 day)

What to write: `LinearRegressionService.cs`

This file wraps `LinearRegressionAlgorithm` with coding rules 3, 4, 7, 8, 9:
- Extends `BaseService`
- `TrainAsync(operationId, datasetName)` — checks idempotency, loads data via `ITrainingDataProvider`, trains, saves via `IModelStateStorage`, dispatches `StandardEvent`
- `PredictAsync(modelName, inputValue)` — loads model, predicts, logs via `IPredictionLogger`

### Phase 3 — Tests (0.5 day)

Unit tests in `Tests/LinearRegressionTests.cs`:
- Train on `[30, 50, 70, 90, 110]` → `[3.0, 5.0, 7.0, 9.0, 11.0]` (same as Python reference), verify slope ≈ 0.1 and intercept ≈ 0.0
- Predict for input 60, verify output ≈ 6.0
- Verify failure on empty input list
- Verify idempotency: call `TrainAsync` twice with same `operationId`, verify `IModelStateStorage.SaveAsync` was called only once

**Acceptance criteria:** all tests pass; output matches Python reference values within 0.001.

**Estimated days:** 3

---

## Sub-Project 15 — Naive Bayes Classifier (Наивный байесовский классификатор)

**What this algorithm does in plain language:** Given a collection of text examples labeled as either "spam" or "not spam" (or any other categories), the algorithm counts how often each word appears in each category. When a new text arrives, it multiplies the probabilities for each word and returns the category with the highest probability. It is called "naive" because it assumes each word is independent of all others — which is not true in practice but works surprisingly well for text classification.

**Reference source:** `ml_algorithms_implementations_en.md` — Section "2. Naive Bayes", Python implementation.

**Dependencies:** None.

**Folder:** `14.MachineLearning/Algorithms/02-NaiveBayes/`

### Phase 1 — Vocabulary and probability tables (1 day)

What to write: `NaiveBayesAlgorithm.cs`

Key data structures (all as `Dictionary<string, object>`):
- `categoryProbabilities` — how often each category appears in training data (e.g. spam=40%, not-spam=60%)
- `wordGivenCategoryProbabilities` — for each category, how often each word appears (as a probability)
- Laplace smoothing (сглаживание Лапласа — добавление единицы к каждому счётчику слов, чтобы слова которые не встречались в обучении не давали нулевую вероятность) to handle words not seen in training

Methods:
- `Train(List<(string text, string category)> examples)` — counts and stores all probabilities
- `Predict(string text)` — returns the most likely category and the confidence percentage

### Phase 2 — Service wrapper (0.5 day)

`NaiveBayesService.cs` extending `BaseService` with `TrainAsync` and `ClassifyAsync`.

### Phase 3 — Tests (0.5 day)

- Train on the spam/ham examples from the Python reference
- Classify `"free get money"` and verify category is spam
- Classify `"meeting tomorrow at office"` and verify category is not-spam
- Verify Laplace smoothing: a word never seen in training should not produce a zero-probability result
- Verify idempotency on `TrainAsync`

**Acceptance criteria:** classification matches Python reference output (spam vs. not-spam for the same test inputs).

**Estimated days:** 2

---

## Sub-Project 16 — Multilayer Perceptron Neural Network (Многослойный перцептрон)

**What this algorithm does in plain language:** A multilayer perceptron is a network of artificial neurons arranged in layers. Data flows forward from the input layer through one or more hidden layers to the output layer (forward pass). After making a prediction, the network measures its error and propagates the error backward through all layers, adjusting each connection weight slightly in the direction that reduces the error (backward error propagation). This is repeated thousands of times until the predictions become accurate.

**Note:** This is a from-scratch implementation of the basic learning algorithm, not TorchSharp. It teaches the mechanics. Sub-project 03 (Transformer Model) already uses TorchSharp for production-grade neural network training. This sub-project implements the same math without any library, using only `double` arrays.

**Reference source:** `ml_algorithms_implementations_en.md` — Section "3. Neural Network (Simple Perceptron)", Python implementation.

**Dependencies:** None.

**Folder:** `14.MachineLearning/Algorithms/03-NeuralNetwork/`

### Phase 1 — Layer and activation functions (1.5 days)

What to write: `NeuralNetworkLayer.cs`, `SigmoidActivation.cs`

```csharp
// SigmoidActivation computes the sigmoid activation function.
// The sigmoid function (сигмоидная функция активации) squashes any real number
// into the range between 0 and 1, making it useful for producing probability-like outputs.
// Formula: sigmoid(x) = 1 / (1 + e^(-x))
public static class SigmoidActivation
{
    // Apply the sigmoid function to a single number
    // input — any real number
    // Returns a number between 0 and 1
    public static double Apply(double input)
    {
        // e^(-input) grows when input is negative and shrinks when input is positive
        // Adding 1 and taking the reciprocal maps everything into (0, 1)
        return 1.0 / (1.0 + Math.Exp(-input));
    }

    // Compute the derivative (производная) of the sigmoid function at a given output value
    // This is used during backward error propagation to know how to adjust weights
    // sigmoidOutput — the output that sigmoid already produced (not the raw input)
    // The derivative formula: sigmoid'(x) = sigmoid(x) × (1 - sigmoid(x))
    public static double Derivative(double sigmoidOutput)
    {
        // When the output is near 0.5, the derivative is largest (weights change most)
        // When the output is near 0 or 1, the derivative is near zero (weights change little)
        return sigmoidOutput * (1.0 - sigmoidOutput);
    }
}
```

`NeuralNetworkLayer.cs` holds one layer's weights (as `double[,]` — a two-dimensional array where rows are neurons in this layer and columns are neurons in the previous layer) and biases (as `double[]`).

### Phase 2 — Forward pass and backward error propagation (2 days)

What to write: `MultilayerPerceptron.cs`

Methods:
- `ForwardPass(double[] inputs)` — computes activations at every layer from input to output; returns the output layer's activations
- `BackwardErrorPropagation(double[] expectedOutputs, double learningRate)` — computes how much each weight contributed to the error and adjusts all weights
- `Train(List<double[]> inputBatch, List<double[]> expectedOutputBatch, int epochCount, double learningRate)` — runs forward and backward pass for each example, for `epochCount` iterations over the full dataset

### Phase 3 — Service wrapper (0.5 day)

`NeuralNetworkService.cs` extending `BaseService`.

### Phase 4 — Tests (0.5 day)

- Train on the XOR problem (Exclusive OR — исключающее ИЛИ: результат 1 только когда ровно один из двух входов равен 1) for 10,000 epochs, verify all four predictions are correct within 0.05
- Verify forward pass produces output in range [0, 1] for any input
- Verify idempotency on `TrainAsync`

**Acceptance criteria:** XOR predictions match Python reference (0 XOR 0 ≈ 0.023, 0 XOR 1 ≈ 0.978).

**Estimated days:** 4.5

---

## Sub-Project 17 — K-Means Clustering (Кластеризация методом K-средних)

**What this algorithm does in plain language:** Given a set of data points and the number of desired groups K, the algorithm starts by placing K cluster centers (centroids) at random positions. Then it repeats two steps until nothing changes: (1) assign every point to the nearest center, (2) move each center to the average position of all points assigned to it. The result is K groups of similar data points.

**Reference source:** `ml_algorithms_implementations_en.md` — Section "4. K-Means (Clustering)", Python implementation.

**Dependencies:** None.

**Folder:** `14.MachineLearning/Algorithms/04-KMeans/`

### Phase 1 — Distance and centroid computation (1 day)

What to write: `EuclideanDistance.cs`, `CentroidCalculator.cs`

```csharp
// EuclideanDistance computes the straight-line distance between two points
// in any number of dimensions (Евклидово расстояние — длина прямой линии
// между двумя точками в многомерном пространстве).
// For two points [x1, y1] and [x2, y2] in 2D: sqrt((x2-x1)² + (y2-y1)²)
public static class EuclideanDistance
{
    // Compute the distance between two points given as arrays of coordinates
    // pointA — coordinates of the first point (e.g. [spending, frequency])
    // pointB — coordinates of the second point (the cluster center to measure against)
    // Both arrays must have the same length (same number of dimensions)
    public static double Compute(double[] pointA, double[] pointB)
    {
        // Verify both points exist in the same dimensional space
        if (pointA.Length != pointB.Length)
            throw new ArgumentException(
                $"Points must have the same number of dimensions. " +
                $"Got {pointA.Length} and {pointB.Length}.");

        // Sum the squared differences along each dimension
        double squaredDifferenceSum = 0;
        for (int dimension = 0; dimension < pointA.Length; dimension++)
        {
            // Difference between the two points along this dimension
            double difference = pointA[dimension] - pointB[dimension];

            // Square it so negative differences become positive and large differences are penalized more
            squaredDifferenceSum += difference * difference;
        }

        // Take the square root to get the actual distance (not the squared distance)
        return Math.Sqrt(squaredDifferenceSum);
    }
}
```

### Phase 2 — Main clustering loop (1.5 days)

What to write: `KMeansAlgorithm.cs`

Methods:
- `Initialize(List<double[]> dataPoints, int clusterCount)` — place starting centroids by picking K random data points as initial centers
- `AssignPointsToClusters(List<double[]> dataPoints)` — for each point, find the nearest centroid and record the assignment
- `RecalculateCentroids(List<double[]> dataPoints, int[] assignments)` — for each cluster, compute the mean of all assigned points
- `Fit(List<double[]> dataPoints, int clusterCount, int maximumIterations)` — run the full loop until assignments stop changing or `maximumIterations` is reached; return cluster assignments and final centroids

### Phase 3 — Service wrapper and tests (1 day)

`KMeansService.cs` extending `BaseService`.

Tests:
- Cluster the customer data from the Python reference (monthly spending, purchase frequency) into 3 groups; verify each cluster contains points that are closer to their own center than to any other center
- Verify that running with the same random seed produces identical results (deterministic output)
- Verify idempotency on `ClusterAsync`

**Acceptance criteria:** all data points are closer to their assigned centroid than to any other centroid (definition of a valid K-means assignment).

**Estimated days:** 3.5

---

## Sub-Project 18 — Apriori Association Rules (Алгоритм Apriori — поиск ассоциативных правил)

**What this algorithm does in plain language:** Given a list of shopping transactions (each transaction is a set of items bought together), the algorithm finds combinations of items that appear together frequently. It then generates rules like "if a customer buys bread, there is a 71% chance they also buy milk." This is used in recommendations, store layout, and cross-selling.

Two key measures:
- Support (поддержка правила) — the fraction of all transactions containing this combination. If bread+milk appears in 5 of 10 transactions, the support is 50%.
- Confidence (уверенность правила) — given that the "if" part was bought, how often was the "then" part also bought. If bread appears in 7 transactions and bread+milk in 5, the confidence is 5/7 = 71%.

**Reference source:** `ml_algorithms_implementations_en.md` — Section "5. Apriori (Association Rules)", Python implementation.

**Dependencies:** None.

**Folder:** `14.MachineLearning/Algorithms/05-Apriori/`

### Phase 1 — Frequent itemset generation (2 days)

**What an itemset means:** a set of items (e.g. `{"bread", "milk"}`) that appears in at least `minimumSupport` fraction of all transactions.

What to write: `FrequentItemsetFinder.cs`

```csharp
// FrequentItemsetFinder implements the Apriori algorithm (алгоритм Apriori).
// The name "Apriori" comes from Latin "a priori" meaning "from what comes before":
// if a set of items is frequent, every smaller subset must also be frequent.
// This property allows pruning: we only extend itemsets we already know are frequent.
public class FrequentItemsetFinder
{
    // Find all combinations of items that appear together in at least minimumSupportFraction
    // of all transactions.
    // transactions — each inner list is one shopping basket (a set of item names)
    // minimumSupportFraction — the minimum fraction of transactions a combination must appear in
    //   (e.g. 0.5 means "appears in at least half of all transactions")
    // Returns a dictionary from itemset (sorted list of item names) to its support fraction
    public OperationResult<Dictionary<List<string>, double>> FindFrequentItemsets(
        List<List<string>> transactions,
        double minimumSupportFraction)
    {
        // ... implementation follows the Apriori candidate generation algorithm ...
    }
}
```

Methods:
- `CountItemFrequencies(transactions)` — count how often each individual item appears; filter out items below the minimum support threshold
- `GenerateCandidatePairs(frequentSingleItems)` — build all two-item combinations from the frequent single items
- `PruneBySupport(candidates, transactions, minimumSupportFraction)` — keep only candidates that appear in enough transactions
- `ExtendToLargerItemsets(frequentItemsets, transactions, minimumSupportFraction)` — repeatedly generate larger candidates and prune until no new frequent sets are found

### Phase 2 — Association rule generation (1 day)

What to write: `AssociationRuleGenerator.cs`

For each frequent itemset of size ≥ 2, generate all possible rules of the form `{A, B} → {C}` and compute their confidence. Keep only rules whose confidence meets the minimum threshold.

### Phase 3 — Service wrapper and tests (0.5 day)

`AprioriService.cs` extending `BaseService`.

Tests:
- Run on the transactions from the Python reference (bread, milk, eggs, cheese, butter combinations)
- Verify the rule `{bread} → {milk}` appears with confidence ≈ 0.71 and support ≈ 0.50
- Verify that no rule has a support fraction below the specified minimum
- Verify idempotency on `FindRulesAsync`

**Acceptance criteria:** rules match the Python reference (same rules, support and confidence within 0.01).

**Estimated days:** 3.5

---

## Sub-Project 19 — Principal Component Analysis (Метод главных компонент)

**What this algorithm does in plain language:** Imagine data with 10 measured properties per item. Most of those properties are correlated — if height increases, weight usually increases too. Principal Component Analysis finds new directions (called principal components or principal directions) along which the data varies the most, and then re-expresses each data point using only the top 2 or 3 of those directions. This reduces 10 dimensions to 2 or 3 while keeping most of the information.

Steps:
1. Subtract the mean of each dimension so the data is centered at zero
2. Build the covariance matrix (матрица ковариации — квадратная матрица показывающая как сильно каждая пара измерений изменяется вместе)
3. Find the principal directions (eigenvectors) and how much variance each one explains (eigenvalues)
4. Project each data point onto the top K principal directions

**Reference source:** `ml_algorithms_implementations_en.md` — Section "6. PCA (Principal Component Analysis)", Python implementation.

**Dependencies:** TorchSharp (for eigenvalue decomposition — разложение матрицы на собственные значения и векторы).

**Folder:** `14.MachineLearning/Algorithms/06-PCA/`

### Phase 1 — Data centering and covariance matrix (1 day)

What to write: `DataCenterer.cs`, `CovarianceMatrixBuilder.cs`

```csharp
// DataCenterer subtracts the mean of each dimension from all data points.
// After centering, each dimension has a mean of zero.
// This is required before computing the covariance matrix.
public static class DataCenterer
{
    // Center a dataset by subtracting the mean of each column
    // dataPoints — each row is one data point; each column is one measured property
    // Returns the centered data and the mean values that were subtracted
    //   (the mean values are needed later to project new data into the same space)
    public static (List<double[]> centeredData, double[] columnMeans) Center(List<double[]> dataPoints)
    {
        // Determine how many dimensions (columns) each data point has
        int dimensionCount = dataPoints[0].Length;

        // Compute the mean of each dimension separately
        var columnMeans = new double[dimensionCount];
        for (int dimension = 0; dimension < dimensionCount; dimension++)
        {
            // Sum all values in this column and divide by the number of points
            columnMeans[dimension] = dataPoints.Average(point => point[dimension]);
        }

        // Subtract the mean from each value so each column now has mean zero
        var centeredData = dataPoints
            .Select(point => point
                .Select((value, dimension) => value - columnMeans[dimension])
                .ToArray())
            .ToList();

        return (centeredData, columnMeans);
    }
}
```

### Phase 2 — Eigenvalue decomposition using TorchSharp (1.5 days)

What to write: `PrincipalComponentComputer.cs`

Uses TorchSharp's `torch.linalg.eigh` (разложение симметричной матрицы на собственные значения и собственные векторы — встроенная функция TorchSharp) to compute eigenvectors and eigenvalues of the covariance matrix. Sorts eigenvectors by descending eigenvalue so the first principal direction explains the most variance.

### Phase 3 — Projection (0.5 day)

What to write: `DimensionalityReducer.cs`

- `Fit(dataPoints, numberOfComponentsToKeep)` — center, compute covariance, find principal directions
- `Transform(dataPoints)` — project each point onto the top principal directions
- `FitAndTransform(dataPoints, numberOfComponentsToKeep)` — convenience method combining both

### Phase 4 — Service wrapper and tests (0.5 day)

`PrincipalComponentAnalysisService.cs` extending `BaseService`.

Tests:
- Reduce the 3D height/weight/age data from the Python reference to 2 dimensions; verify that the explained variance of the first two components is ≥ 95% of the total
- Verify the output shape: N data points projected to 2 dimensions gives a list of N 2-element arrays
- Verify idempotency on `ReduceDimensionsAsync`

**Acceptance criteria:** variance retained after reduction matches Python reference within 1%.

**Estimated days:** 3.5

---

## Sub-Project 20 — Q-Learning Reinforcement (Обучение с подкреплением методом Q-таблиц)

**What this algorithm does in plain language:** An agent moves through an environment (for example, a grid maze) and learns by trial and error. Each time it takes an action in a state, it receives a reward (positive for reaching the goal, negative for hitting a wall). The agent records, for every (state, action) pair, the expected total future reward — this is the "Q-value" or "action value." Over thousands of episodes, the Q-values converge to accurate estimates and the agent learns the optimal path.

**Key concepts:**
- Epsilon-greedy exploration (баланс между исследованием и использованием знаний): with probability `explorationProbability` (epsilon) the agent picks a random action to discover new paths; otherwise it picks the action with the highest Q-value
- Discount factor (коэффициент дисконтирования): future rewards are worth less than immediate ones; a discount factor of 0.9 means a reward 2 steps away is worth 0.9² = 0.81 compared to an immediate reward of 1.0
- Q-table update rule (формула обновления Q-таблицы): `Q(state, action) = Q(state, action) + learningRate × (reward + discountFactor × maxQ(nextState) - Q(state, action))`

**Reference source:** `ml_algorithms_implementations_en.md` — Section "7. Q-Learning", Python implementation.

**Dependencies:** None.

**Folder:** `14.MachineLearning/Algorithms/07-QLearning/`

### Phase 1 — Environment interface and Q-table (1 day)

What to write: `IEnvironment.cs`, `ActionValueTable.cs`

```csharp
// IEnvironment describes any world the Q-learning agent can explore.
// The agent does not know whether the environment is a maze, a game, or a factory floor —
// it only knows how to ask the environment for the current state and what reward it gets.
public interface IEnvironment
{
    // Return the state the agent is currently in
    // A state is represented as a single integer (the position in the maze, the game score, etc.)
    int GetCurrentState();

    // Return a list of all possible actions the agent can take from the current state
    // Actions are represented as integers (e.g. 0=up, 1=down, 2=left, 3=right)
    List<int> GetAvailableActions();

    // Execute an action: move to the new state and return the reward received
    // action — the integer representing which action to take
    // Returns: (new state after taking the action, reward received for taking this action)
    (int newState, double reward) TakeAction(int action);

    // Return whether the agent has reached a terminal state (goal reached or episode over)
    bool IsTerminal();

    // Reset the environment to its starting state for a new episode
    void Reset();
}
```

`ActionValueTable.cs` — stores Q-values as `Dictionary<(int state, int action), double>` with a default value of 0 for unseen pairs.

### Phase 2 — Q-learning update algorithm (1 day)

What to write: `QLearningAlgorithm.cs`

```csharp
// QLearningAlgorithm implements the Q-learning update rule.
// This is the core of the learning algorithm — it adjusts Q-values based on experience.
public class QLearningAlgorithm
{
    // The table mapping (state, action) pairs to their expected total future reward
    private readonly ActionValueTable actionValueTable;

    // How fast the agent updates its Q-values based on new experience
    // Higher learningRate = faster but less stable learning
    private readonly double learningRate;

    // How much less a future reward is worth compared to an immediate reward
    // A discountFactor of 0.9 means a reward 1 step away is worth 90% of an immediate reward
    private readonly double discountFactor;

    // Update the Q-value for a (state, action) pair after observing the result
    // currentState — the state the agent was in before taking the action
    // actionTaken — which action the agent chose
    // rewardReceived — the reward the environment gave for this action
    // nextState — the state the agent ended up in after taking the action
    public void UpdateQValue(int currentState, int actionTaken, double rewardReceived, int nextState)
    {
        // The current estimate of value for this (state, action) pair
        double currentQValue = actionValueTable.Get(currentState, actionTaken);

        // The best Q-value achievable from the next state (what the agent can get from here onward)
        double bestFutureQValue = actionValueTable.GetBestActionValue(nextState);

        // The "target" — what the Q-value should ideally be based on this experience
        // Immediate reward plus discounted best future value
        double targetQValue = rewardReceived + discountFactor * bestFutureQValue;

        // Move the current Q-value partway toward the target, scaled by the learning rate
        // The learning rate determines how big a step we take in the direction of the target
        double updatedQValue = currentQValue + learningRate * (targetQValue - currentQValue);

        // Store the updated Q-value in the table
        actionValueTable.Set(currentState, actionTaken, updatedQValue);
    }
}
```

### Phase 3 — Training loop and epsilon-greedy action selection (1 day)

What to write: `QLearningTrainer.cs`

- `SelectAction(int currentState, double explorationProbability)` — pick a random action with probability `explorationProbability`, otherwise pick the action with the highest Q-value
- `RunTrainingEpisode(environment, explorationProbability)` — reset environment, run until terminal, update Q-values at each step
- `Train(environment, episodeCount, explorationProbability, learningRate, discountFactor)` — run `episodeCount` training episodes

### Phase 4 — Service wrapper and maze test (1 day)

`QLearningService.cs` extending `BaseService`.

What to write: `GridMazeEnvironment.cs` — a concrete implementation of `IEnvironment` for the simple 4×4 maze from the Python reference. Used only in tests.

Tests:
- Train on the maze for 1,000 episodes; verify the agent can then navigate from start to goal in ≤ 10 steps (the maze has an optimal path of 6 steps)
- Verify the Q-table assigns the highest value to the action that moves toward the goal from the starting state
- Verify idempotency on `TrainAsync`

**Acceptance criteria:** the trained agent consistently reaches the goal in a small number of steps, matching the Python reference behavior.

**Estimated days:** 4

---

## Part 5 — Shared Dispatcher (Coding Rule 6)

```csharp
// MachineLearningDispatcher routes any incoming operation request to the correct algorithm service.
// It implements coding rule 6: one generic dispatcher instead of seven separate controllers.
// Callers say "train algorithm X on dataset Y" — the dispatcher figures out which service to call.
public class MachineLearningDispatcher : BaseService
{
    // All seven algorithm services, injected by the dependency injection container
    // The dispatcher does not know their concrete types — only their shared interface
    private readonly Dictionary<string, IAlgorithmService> algorithmServices;

    // Route an incoming train request to the correct algorithm
    // algorithmName — e.g. "LinearRegression", "NaiveBayes", "KMeans"
    // operationId — unique identifier for idempotency checking
    // datasetName — which dataset to load via ITrainingDataProvider
    public async Task<OperationResult<Dictionary<string, object>>> TrainAsync(
        string algorithmName,
        string operationId,
        string datasetName)
    {
        // Look up the algorithm service by name
        if (!algorithmServices.TryGetValue(algorithmName, out var service))
        {
            // No service registered for this algorithm name — return a clear error
            return OperationResult<Dictionary<string, object>>.Fail(
                $"No algorithm registered with the name '{algorithmName}'. " +
                $"Available algorithms: {string.Join(", ", algorithmServices.Keys)}",
                "ALGORITHM_NOT_FOUND");
        }

        // Delegate training to the found service
        return await service.TrainAsync(operationId, datasetName);
    }
}
```

The `IAlgorithmService` interface is defined in `Fabric/IAlgorithmService.cs` with two methods: `TrainAsync` and `PredictAsync`.

---

## Part 6 — Plan Review Checks

Before this plan is handed to any implementation session, the following checks must pass:

**Goal delivery check:** every algorithm listed in the algorithms document is covered by a sub-project in this plan. Seven algorithms — seven sub-projects. All seven goals are mapped to implementation phases with verification steps. ✓

**Design artifact completeness check:** the reference source (`ml_algorithms_implementations_en.md`) is named and the specific sections are referenced for each sub-project. ✓

**Authentication check:** no HTTP controllers are produced in this plan. The `MachineLearningDispatcher` is an internal C# class, not an HTTP endpoint. No authentication declarations needed. ✓

**NDJSON type coverage check:** no arbiter fixture files are produced in this plan. Not applicable. ✓

**Definition-of-done check:** each sub-project defines acceptance criteria with verifiable test outcomes. Gate A (build + tests), Gate B (output matches Python reference), Gate C (explicit approval) apply to each sub-project. ✓

**Count accuracy check:** 7 algorithms × 1 sub-project each = sub-projects 14 through 20. No number drift. ✓

---

## Part 7 — Updated Solution Structure After This Plan

```
LlmFromScratch.sln
├── 01.Tokenizer/
├── 02.DatasetPipeline/
├── 03.TransformerModel/
├── 04.TrainingLoop/
├── 05.Embeddings/
├── 06.VectorSearch/
├── 07.GraphEngine/
├── 08.GraphRag/
├── 09.Inference/
├── 10.LoRA/
├── 11.DPO/
├── 12.SearchEngine/
├── 13.CodeOptimizer/
└── 14.MachineLearning/          ← this plan
    ├── Algorithms/
    │   ├── 01-LinearRegression/     sub-project 14
    │   ├── 02-NaiveBayes/           sub-project 15
    │   ├── 03-NeuralNetwork/        sub-project 16
    │   ├── 04-KMeans/               sub-project 17
    │   ├── 05-Apriori/              sub-project 18
    │   ├── 06-PCA/                  sub-project 19
    │   └── 07-QLearning/            sub-project 20
    ├── Fabric/
    ├── Core/
    ├── Dispatcher/
    └── Tests/
```

---

## Part 8 — Dependency Map and Build Order

All seven algorithm implementations inside sub-project 14 are independent of each other. They can be built and tested in any order. The suggested order follows increasing mathematical complexity:

```
Phase A (independent, can be done in any order or parallel):
  Sub-project 14 — Linear Regression         (3 days)
  Sub-project 15 — Naive Bayes               (2 days)
  Sub-project 17 — K-Means                   (3.5 days)

Phase B (slightly more complex, after Phase A is familiar):
  Sub-project 16 — Multilayer Perceptron     (4.5 days)
  Sub-project 18 — Apriori                   (3.5 days)
  Sub-project 19 — Principal Component Analysis (3.5 days)

Phase C (most complex learning loop):
  Sub-project 20 — Q-Learning                (4 days)
```

---

## Part 9 — Summary Table

| Sub-project | Algorithm full name | Days | Gate B verification |
|---|---|---|---|
| 14 | Linear Regression (Линейная регрессия) | 3 | slope ≈ 0.1, predict(60) ≈ 6.0 |
| 15 | Naive Bayes Classifier (Наивный байесовский классификатор) | 2 | "free get money" → spam |
| 16 | Multilayer Perceptron Neural Network (Многослойный перцептрон) | 4.5 | XOR all 4 predictions correct ±0.05 |
| 17 | K-Means Clustering (Кластеризация методом K-средних) | 3.5 | all points closer to own centroid |
| 18 | Apriori Association Rules (Алгоритм Apriori) | 3.5 | bread→milk confidence ≈ 0.71 |
| 19 | Principal Component Analysis (Метод главных компонент) | 3.5 | ≥95% variance retained in 2D |
| 20 | Q-Learning Reinforcement (Обучение с подкреплением) | 4 | agent reaches goal in ≤10 steps |
| **Total** | | **24 days** | |

---

## Part 10 — Pre-Commit Checklist (same as main plan v2.0, copied here for convenience)

Before any file is considered complete:

```
□ Every public method returns OperationResult<T>                          (coding rule 3)
□ Every service class extends BaseService                                   (coding rule 4)
□ No scopeId, userId, or tenantId parameter in any method signature        (coding rule 5)
□ No new() call on a fabric implementation inside a service                (fabric-first)
□ Every line of code carries a plain-language comment in Russian
□ No abbreviation without the full form on the same line
□ Variable names are fully spelled out (no lr, gamma, eps, nb, mlp, pca)
□ dotnet build exits 0, no warnings
□ All unit tests pass
□ Gate B: output matches Python reference within stated tolerance
```

---

*Plan version 1.0 — covers sub-projects 14 through 20 (seven classic machine learning algorithms). Does not touch sub-projects 01–13 or XIIGen work.*
