using TensorCore.Core;
using TensorCore.Layers;
using TransformerModel.Config;
using TransformerModel.Layers;

namespace TransformerModel;

// Один декодерный блок трансформера:
//   x = x + attention(norm1(x))
//   x = x + ffn(norm2(x))
// Residual connections позволяют градиентам течь через 32+ стека без затухания.
public class TransformerBlock : Module
{
    private readonly RmsNorm             _norm1;
    private readonly CausalSelfAttention _attn;
    private readonly RmsNorm             _norm2;
    private readonly SwiGLUFeedForward   _ffn;

    // Открытый доступ для LoraApplicator
    public CausalSelfAttention Attention => _attn;

    public TransformerBlock(ModelConfig cfg)
    {
        _norm1 = new RmsNorm(cfg.HiddenDim, cfg.RmsNormEps);
        _attn  = new CausalSelfAttention(cfg);
        _norm2 = new RmsNorm(cfg.HiddenDim, cfg.RmsNormEps);
        _ffn   = new SwiGLUFeedForward(cfg);

        RegisterModule("norm1", _norm1);
        RegisterModule("attn",  _attn);
        RegisterModule("norm2", _norm2);
        RegisterModule("ffn",   _ffn);
    }

    public Tensor Forward(Tensor x)
    {
        // Pre-norm резидуальный блок: нормируем перед операцией
        x = TensorOps.Add(x, _attn.Forward(_norm1.Forward(x)));
        x = TensorOps.Add(x, _ffn.Forward(_norm2.Forward(x)));
        return x;
    }
}
