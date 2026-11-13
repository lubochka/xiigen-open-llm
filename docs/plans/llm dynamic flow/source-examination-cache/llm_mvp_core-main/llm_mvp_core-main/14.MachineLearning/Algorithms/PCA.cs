namespace MachineLearning;

// Principal Component Analysis — reduces dimensionality by projecting data
// onto the directions of maximum variance (principal components).
// Uses power iteration to find the top-k eigenvectors of the covariance matrix.
public sealed class PcaAlgorithm
{
    private double[]   _mean        = [];
    private double[,]  _components  = new double[0, 0]; // [numComponents, inputDim]
    private double[]   _eigenvalues = [];
    private bool _fitted;

    public OperationResult<bool> Fit(List<double[]> data, int numComponents)
    {
        if (data.Count == 0)
            return OperationResult<bool>.Fail("Empty dataset");

        int n   = data.Count;
        int dim = data[0].Length;

        if (numComponents > dim)
            return OperationResult<bool>.Fail($"numComponents={numComponents} > dim={dim}");

        // Center the data
        _mean = new double[dim];
        foreach (var row in data)
            for (int d = 0; d < dim; d++)
                _mean[d] += row[d] / n;

        var centered = data.Select(row =>
        {
            var r = new double[dim];
            for (int d = 0; d < dim; d++) r[d] = row[d] - _mean[d];
            return r;
        }).ToList();

        // Covariance matrix C = X'X / (n-1)
        var C = new double[dim, dim];
        foreach (var row in centered)
            for (int i = 0; i < dim; i++)
                for (int j = 0; j < dim; j++)
                    C[i, j] += row[i] * row[j] / (n - 1);

        // Power iteration deflation to find top-k eigenpairs
        _components  = new double[numComponents, dim];
        _eigenvalues = new double[numComponents];
        var rng      = new Random(42);

        for (int k = 0; k < numComponents; k++)
        {
            // Random init
            var v = new double[dim];
            for (int d = 0; d < dim; d++) v[d] = rng.NextDouble() - 0.5;
            Normalise(v);

            for (int iter = 0; iter < 200; iter++)
            {
                var w = MatVec(C, v);
                double eigenvalue = Dot(v, w);
                Normalise(w);
                if (Dot(v, w) > 1 - 1e-9 && iter > 10) break;
                v = w;
            }

            double ev = Dot(v, MatVec(C, v));
            _eigenvalues[k] = ev;
            for (int d = 0; d < dim; d++)
                _components[k, d] = v[d];

            // Deflate: C -= eigenvalue * v * v'
            for (int i = 0; i < dim; i++)
                for (int j = 0; j < dim; j++)
                    C[i, j] -= ev * v[i] * v[j];
        }

        _fitted = true;
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<double[]> Transform(double[] point)
    {
        if (!_fitted) return OperationResult<double[]>.Fail("Model not fitted");
        int k   = _components.GetLength(0);
        int dim = _components.GetLength(1);
        var centered = new double[dim];
        for (int d = 0; d < dim; d++) centered[d] = point[d] - _mean[d];

        var projected = new double[k];
        for (int i = 0; i < k; i++)
        {
            double dot = 0;
            for (int d = 0; d < dim; d++) dot += _components[i, d] * centered[d];
            projected[i] = dot;
        }
        return OperationResult<double[]>.Ok(projected);
    }

    public IReadOnlyList<double> ExplainedVarianceRatio
    {
        get
        {
            double total = _eigenvalues.Sum();
            return total == 0
                ? _eigenvalues.Select(_ => 0.0).ToList()
                : _eigenvalues.Select(e => e / total).ToList();
        }
    }

    private static double[] MatVec(double[,] M, double[] v)
    {
        int n = v.Length;
        var r = new double[n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                r[i] += M[i, j] * v[j];
        return r;
    }

    private static double Dot(double[] a, double[] b)
    {
        double s = 0;
        for (int i = 0; i < a.Length; i++) s += a[i] * b[i];
        return s;
    }

    private static void Normalise(double[] v)
    {
        double norm = Math.Sqrt(Dot(v, v));
        if (norm < 1e-12) return;
        for (int i = 0; i < v.Length; i++) v[i] /= norm;
    }
}
