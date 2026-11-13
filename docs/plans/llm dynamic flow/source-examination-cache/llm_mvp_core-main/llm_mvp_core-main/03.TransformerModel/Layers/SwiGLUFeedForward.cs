using TensorCore.Core;
using TensorCore.Layers;
using TransformerModel.Config;

namespace TransformerModel.Layers;

// SwiGLU Feed-Forward Network.
// gate = silu(x @ gate_proj^T) — плавный вентиль: что пропускать
// up   = x @ up_proj^T
// out  = (gate * up) @ down_proj^T
// SwiGLU работает лучше ReLU в трансформерах (PaLM, LLaMA, Mistral).
public class SwiGLUFeedForward : Module
{
    private readonly Linear _gateProj;
    private readonly Linear _upProj;
    private readonly Linear _downProj;

    public SwiGLUFeedForward(ModelConfig cfg)
    {
        int h   = cfg.HiddenDim;
        int ffn = cfg.FfnHiddenDim;

        _gateProj = new Linear(h, ffn, useBias: false);
        _upProj   = new Linear(h, ffn, useBias: false);
        _downProj = new Linear(ffn, h, useBias: false);

        RegisterModule("gate_proj", _gateProj);
        RegisterModule("up_proj",   _upProj);
        RegisterModule("down_proj", _downProj);
    }

    public Tensor Forward(Tensor x)
    {
        // gate активируется SiLU, up проходит без активации, перемножаются поэлементно
        var gate = TensorOps.Silu(_gateProj.Forward(x));
        var up   = _upProj.Forward(x);
        return _downProj.Forward(TensorOps.Multiply(gate, up));
    }
}
