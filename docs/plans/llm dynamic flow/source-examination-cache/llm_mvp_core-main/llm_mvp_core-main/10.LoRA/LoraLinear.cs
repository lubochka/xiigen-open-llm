using TensorCore.Core;
using TensorCore.Init;
using TensorCore.Layers;

namespace LoRA;

// LoRA адаптер поверх замороженного Linear слоя.
// Output = frozen_linear(x) + scaling * (x @ A^T) @ B^T
// A инициализируется Kaiming; B = 0 (адаптер начинает как нулевая поправка).
public class LoraLinear : Module
{
    private readonly Linear    _base;
    private readonly Parameter _loraA;
    private readonly Parameter _loraB;
    private readonly Dropout   _dropout;
    private readonly float     _scaling;

    public LoraLinear(Linear baseLinear, LoraConfig config)
    {
        _base    = baseLinear;
        _scaling = config.Scaling;
        _dropout = new Dropout(config.Dropout);

        int inDim  = baseLinear.InFeatures;
        int outDim = baseLinear.OutFeatures;

        // A — матрица проекции в низкоранговое пространство
        var aData = Initializers.KaimingUniform(config.Rank, inDim);
        _loraA = new Parameter(new Tensor(aData, new TensorShape(config.Rank, inDim)));

        // B — матрица расширения обратно, инициализируется нулями
        var bData = Initializers.Zeros(outDim, config.Rank);
        _loraB = new Parameter(new Tensor(bData, new TensorShape(outDim, config.Rank)));

        // Замораживаем базовые веса
        baseLinear.Weight.Value.RequiresGrad = false;
        if (baseLinear.Bias != null)
            baseLinear.Bias.Value.RequiresGrad = false;

        RegisterModule("base",    _base);
        RegisterModule("dropout", _dropout);
        RegisterParameter("lora_a", _loraA);
        RegisterParameter("lora_b", _loraB);
    }

    // output = base(x) + scaling * dropout(x) @ A^T @ B^T
    public Tensor Forward(Tensor x)
    {
        var baseOut = _base.Forward(x);

        // Low-rank путь: x → dropout → @ A^T → @ B^T → * scaling
        var xDrop = _dropout.Forward(x, training: true);
        var aT    = TensorOps.Transpose(_loraA.Value, 0, 1); // [in, rank]
        var bT    = TensorOps.Transpose(_loraB.Value, 0, 1); // [rank, out]
        var lora  = TensorOps.MatMul(TensorOps.MatMul(xDrop, aT), bT);
        lora      = TensorOps.Scale(lora, _scaling);

        return TensorOps.Add(baseOut, lora);
    }
}
