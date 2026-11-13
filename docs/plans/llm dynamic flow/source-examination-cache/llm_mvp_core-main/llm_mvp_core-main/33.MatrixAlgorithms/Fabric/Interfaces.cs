namespace MatrixAlgorithms;

// ─── Domain fabric interfaces ─────────────────────────────────────────────
// These interfaces define the contracts that all services in this sub-project
// implement or depend on. Tests inject in-memory implementations; real code
// wires concrete implementations at the composition root.

// IMatrixDecomposer is the single interface every decomposition algorithm implements.
// Whether LU, QR, or singular value decomposition — they all take a flat matrix
// and return a DecompositionResult that carries the factor matrices.
public interface IMatrixDecomposer
{
    // Decompose the given matrix into its factor matrices.
    // matrix — all entries in row-major order (row 0 first, then row 1, etc.)
    // rows    — number of rows in the matrix
    // columns — number of columns in the matrix
    // Returns the factor matrices and an algorithm name; fails for singular matrices.
    OperationResult<DecompositionResult> Decompose(double[] matrix, int rows, int columns);
}

// ISparseMatrixStorage saves and loads named sparse matrices without materialising zeros.
// Any component that needs to persist a graph adjacency matrix or a sparse weight matrix
// uses this interface instead of writing to disk directly (fabric-first rule).
public interface ISparseMatrixStorage
{
    // Save a sparse matrix under a human-readable name for later retrieval.
    OperationResult<bool> Save(string name, SparseMatrixData data);

    // Load a previously saved sparse matrix by its name.
    OperationResult<SparseMatrixData> Load(string name);
}

// IWeightCompressor compresses a large dense neural network weight matrix
// into a compact low-rank form that uses much less memory while preserving accuracy.
public interface IWeightCompressor
{
    // Compress the weight matrix by keeping only the targetRank most important directions.
    // weightMatrix — all entries in row-major order
    // rows, columns — original matrix dimensions
    // targetRank — how many singular values (directions of importance) to keep
    OperationResult<CompressedWeightMatrix> Compress(
        double[] weightMatrix, int rows, int columns, int targetRank);

    // Reconstruct the approximate full matrix from the compressed form.
    // The result is close to the original but not identical (lossy compression).
    OperationResult<double[]> Decompress(CompressedWeightMatrix compressed);
}

// ─── In-memory implementations for tests ─────────────────────────────────
// These are test doubles (not mocks — they have real behaviour): they implement
// the interfaces using memory structures instead of files or databases.

// InMemoryMatrixStorage implements IMatrixStorage using a plain dictionary.
// Tests use this as the storage argument when constructing services under test.
public sealed class InMemoryMatrixStorage : IMatrixStorage
{
    // Simple dictionary from string key to the stored payload dictionary.
    private readonly Dictionary<string, Dictionary<string, object>> _store = [];

    // Save a payload under the given key; overwrites any previous value.
    public bool TrySave(string key, Dictionary<string, object> data)
    {
        _store[key] = data;
        return true;
    }

    // Retrieve a payload by key; returns false when the key does not exist.
    public bool TryLoad(string key, out Dictionary<string, object> data)
        => _store.TryGetValue(key, out data!);
}

// InMemoryMatrixEventLogger implements IMatrixEventLogger by collecting
// all logged events in a list. Tests inspect this list after the fact.
public sealed class InMemoryMatrixEventLogger : IMatrixEventLogger
{
    // All events logged during the test, in the order they were emitted.
    public List<StandardEvent> LoggedEvents { get; } = [];

    // Append the event to the list; does not throw or block.
    public void Log(StandardEvent evt) => LoggedEvents.Add(evt);
}

// InMemorySparseMatrixStorage implements ISparseMatrixStorage using a dictionary.
// Tests use this to verify that save/load round-trips preserve all non-zero data.
public sealed class InMemorySparseMatrixStorage : ISparseMatrixStorage
{
    // Map from human-readable name to the stored sparse matrix data.
    private readonly Dictionary<string, SparseMatrixData> _store = [];

    // Save the sparse matrix under the given name.
    public OperationResult<bool> Save(string name, SparseMatrixData data)
    {
        _store[name] = data;
        return OperationResult<bool>.Ok(true);
    }

    // Load a previously saved sparse matrix; fails if the name is not known.
    public OperationResult<SparseMatrixData> Load(string name)
        => _store.TryGetValue(name, out var data)
            ? OperationResult<SparseMatrixData>.Ok(data)
            : OperationResult<SparseMatrixData>.Fail(
                $"No sparse matrix named '{name}' was found in storage.", "NOT_FOUND");
}
