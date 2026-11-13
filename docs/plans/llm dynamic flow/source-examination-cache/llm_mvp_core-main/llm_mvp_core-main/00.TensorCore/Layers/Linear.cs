using TensorCore.Core;
using TensorCore.Init;

namespace TensorCore.Layers;

// Линейный слой: output = input @ weight^T + bias.
// Аналог torch.nn.Linear.
public class Linear : Module
{
    // Матрица весов [outFeatures, inFeatures]; транспонируется при умножении
    public Parameter Weight { get; }

    // Вектор смещения [outFeatures]; null если useBias=false
    public Parameter? Bias { get; }

    public int InFeatures  { get; }
    public int OutFeatures { get; }

    public Linear(int inFeatures, int outFeatures, bool useBias = true)
    {
        InFeatures  = inFeatures;
        OutFeatures = outFeatures;

        // Kaiming Uniform — стандартная инициализация для сетей с ReLU
        var weightData = Initializers.KaimingUniform(outFeatures, inFeatures);
        Weight = new Parameter(new Tensor(weightData, new TensorShape(outFeatures, inFeatures)));
        RegisterParameter("weight", Weight);

        if (useBias)
        {
            Bias = new Parameter(new Tensor(new float[outFeatures], new TensorShape(outFeatures)));
            RegisterParameter("bias", Bias);
        }
    }

    // Прямой проход: [..., inFeatures] → [..., outFeatures]
    public Tensor Forward(Tensor input)
    {
        // input @ weight^T: вес транспонируется с [out, in] → [in, out]
        var weightT = TensorOps.Transpose(Weight.Value, 0, 1);
        var output  = TensorOps.MatMul(input, weightT);

        if (Bias != null)
        {
            // Расширяем bias для broadcast сложения
            var biasExpanded = ExpandBias(Bias.Value, output.Shape);
            output = TensorOps.Add(output, biasExpanded);
        }

        return output;
    }

    // Повторяем вектор [outFeatures] по всем позициям батча/seq
    private static Tensor ExpandBias(Tensor bias, TensorShape outputShape)
    {
        var data    = new float[outputShape.TotalElements];
        int outSize = outputShape.Dimensions[^1];

        // Количество повторений = все размерности кроме последней
        int repeats = outputShape.TotalElements / outSize;

        for (int r = 0; r < repeats; r++)
        for (int o = 0; o < outSize; o++)
            data[r * outSize + o] = bias.Data[o];

        // Bias не участвует в вычислении градиента через этот путь —
        // его градиент накапливается через RegisterParameter
        return new Tensor(data, outputShape);
    }
}
