using TensorCore.Core;
using TensorCore.Init;
using TensorCore.Layers;

namespace TransformerModel.Layers;

// RMS Layer Normalisation: x / sqrt(mean(x²) + eps) * gamma
// Проще чем LayerNorm (нет вычитания среднего), но столь же эффективна на практике.
public class RmsNorm : Module
{
    // Обучаемый масштабирующий вектор инициализируется единицами
    private readonly Parameter _gamma;
    private readonly float _eps;

    public RmsNorm(int dim, float eps)
    {
        _eps   = eps;
        _gamma = new Parameter(new Tensor(Initializers.Ones(dim), new TensorShape(dim)));
        RegisterParameter("gamma", _gamma);
    }

    // x: [..., dim] → нормализованный [..., dim]
    public Tensor Forward(Tensor x)
    {
        var sq   = TensorOps.Pow(x, 2f);
        var mean = TensorOps.Mean(sq, axis: -1, keepDims: true);      // [..., 1]
        var rms  = TensorOps.RSqrt(TensorOps.AddScalar(mean, _eps));  // 1/sqrt(mean+eps)

        // Умножить x на rms: broadcast [...,1] по последней оси
        var normalized = MultiplyBroadcast(x, rms);

        // Умножить на gamma [dim] по последней оси
        return MultiplyGamma(normalized, _gamma.Value);
    }

    // [..., dim] * [..., 1]  — поэлементный broadcast по последней оси
    private static Tensor MultiplyBroadcast(Tensor x, Tensor rms)
    {
        var data    = new float[x.Shape.TotalElements];
        int lastDim = x.Shape.Dimensions[^1];
        int n       = x.Shape.TotalElements / lastDim;

        for (int i = 0; i < n; i++)
        {
            float scale = rms.Data[i];
            for (int j = 0; j < lastDim; j++)
                data[i * lastDim + j] = x.Data[i * lastDim + j] * scale;
        }

        return new Tensor(data, x.Shape);
    }

    // [..., dim] * gamma[dim]  — broadcast gamma по всем батчам/позициям
    private static Tensor MultiplyGamma(Tensor x, Tensor gamma)
    {
        var data = new float[x.Shape.TotalElements];
        int dim  = gamma.Shape.Dimensions[0];
        for (int i = 0; i < x.Shape.TotalElements; i++)
            data[i] = x.Data[i] * gamma.Data[i % dim];
        return new Tensor(data, x.Shape);
    }
}
