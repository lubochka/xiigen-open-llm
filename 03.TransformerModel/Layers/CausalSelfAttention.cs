// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Layers;
using TransformerModel.Config;

namespace TransformerModel.Layers;

// Causal (masked) multi-head self-attention with Grouped Query Attention (GQA).
// GQA: Q uses NumHeads heads, K and V share NumKvHeads heads — saves memory.
// "Causal" — each token sees only itself and previous tokens (no future leakage).
public class CausalSelfAttention : Module
{
    private readonly Linear _qProj;
    private readonly Linear _kProj;
    private readonly Linear _vProj;
    private readonly Linear _oProj;
    private readonly RotaryEmbedding _rope;
    private readonly ModelConfig _cfg;

    public CausalSelfAttention(ModelConfig cfg)
    {
        _cfg = cfg;
        int h  = cfg.HiddenDim;
        int hd = cfg.HeadDim;

        _qProj = new Linear(h, cfg.NumHeads   * hd, useBias: false);
        _kProj = new Linear(h, cfg.NumKvHeads * hd, useBias: false);
        _vProj = new Linear(h, cfg.NumKvHeads * hd, useBias: false);
        _oProj = new Linear(h, h,                  useBias: false);
        _rope  = new RotaryEmbedding(hd, cfg.RopeTheta);

        RegisterModule("q_proj", _qProj);
        RegisterModule("k_proj", _kProj);
        RegisterModule("v_proj", _vProj);
        RegisterModule("o_proj", _oProj);
        RegisterModule("rope",   _rope);
    }

    public Tensor Forward(Tensor x)
    {
        int batch = x.Shape.Dimensions[0];
        int seq   = x.Shape.Dimensions[1];
        int h     = _cfg.NumHeads;
        int kv    = _cfg.NumKvHeads;
        int hd    = _cfg.HeadDim;

        // Project into Q, K, V and split into heads
        // [B, T, h*hd] → reshape → [B, h, T, hd]
        var q = Reshape4D(_qProj.Forward(x), batch, seq, h,  hd);
        var k = Reshape4D(_kProj.Forward(x), batch, seq, kv, hd);
        var v = Reshape4D(_vProj.Forward(x), batch, seq, kv, hd);

        // Apply RoPE to Q and K
        var (cos, sin) = _rope.GetFrequencies(seq);
        q = RotaryEmbedding.Apply(q, cos, sin);
        k = RotaryEmbedding.Apply(k, cos, sin);

        // GQA: expand K and V to h heads (repeat each head KvGroupSize times)
        if (kv < h)
        {
            int g = _cfg.KvGroupSize;
            k = TensorOps.RepeatInterleave(k, g, dim: 1);
            v = TensorOps.RepeatInterleave(v, g, dim: 1);
        }

        // Scaled dot-product attention with a causal mask
        var attnOut = ScaledDotProduct(q, k, v, seq);

        // [B, h, T, hd] → [B, T, H] → _oProj
        var combined = Reshape3D(attnOut, batch, seq, h * hd);
        return _oProj.Forward(combined);
    }

    // Scaled dot-product attention:
    // scores = Q @ K^T / sqrt(hd), apply the causal mask, softmax, @ V
    private static Tensor ScaledDotProduct(Tensor q, Tensor k, Tensor v, int seq)
    {
        float scale = MathF.Sqrt(q.Shape.Dimensions[3]);

        // scores: [B, h, T, T]
        var kT     = TensorOps.Transpose(k, -2, -1);          // [B, h, hd, T]
        var scores = TensorOps.MatMul(q, kT);                 // [B, h, T, T]
        scores     = TensorOps.Scale(scores, 1f / scale);

        // Causal mask: fill the positions above the diagonal with -inf
        var tril = TensorOps.Tril(seq);                       // [T, T]
        scores = ApplyCausalMask(scores, tril, seq);

        // Softmax over the last axis (over the keys)
        var weights = TensorOps.Softmax(scores, dim: -1);     // [B, h, T, T]
        return TensorOps.MatMul(weights, v);                   // [B, h, T, hd]
    }

    // Fill positions outside the lower triangle with -inf
    private static Tensor ApplyCausalMask(Tensor scores, Tensor tril, int seq)
    {
        int batch  = scores.Shape.Dimensions[0];
        int heads  = scores.Shape.Dimensions[1];
        var data   = scores.Data.ToArray();

        for (int b = 0; b < batch; b++)
        for (int h = 0; h < heads; h++)
        for (int i = 0; i < seq; i++)
        for (int j = 0; j < seq; j++)
        {
            if (tril.Data[i * seq + j] == 0f)
            {
                int idx = ((b * heads + h) * seq + i) * seq + j;
                data[idx] = float.NegativeInfinity;
            }
        }

        return new Tensor(data, scores.Shape);
    }

    // [B, T, heads*hd] → [B, heads, T, hd]
    private static Tensor Reshape4D(Tensor x, int batch, int seq, int heads, int hd)
    {
        // x is already [B, T, heads*hd], rearrange to [B, heads, T, hd]
        var data = new float[batch * heads * seq * hd];

        for (int b = 0; b < batch; b++)
        for (int t = 0; t < seq; t++)
        for (int h = 0; h < heads; h++)
        for (int d = 0; d < hd; d++)
        {
            float val = x.Data[((b * seq + t) * heads + h) * hd + d];
            data[((b * heads + h) * seq + t) * hd + d] = val;
        }

        return new Tensor(data, new TensorShape(batch, heads, seq, hd));
    }

    // [B, heads, T, hd] → [B, T, heads*hd]
    private static Tensor Reshape3D(Tensor x, int batch, int seq, int total)
    {
        int heads = x.Shape.Dimensions[1];
        int hd    = x.Shape.Dimensions[3];
        var data  = new float[batch * seq * total];

        for (int b = 0; b < batch; b++)
        for (int h = 0; h < heads; h++)
        for (int t = 0; t < seq; t++)
        for (int d = 0; d < hd; d++)
        {
            float val = x.Data[((b * heads + h) * seq + t) * hd + d];
            data[((b * seq + t) * heads + h) * hd + d] = val;
        }

        return new Tensor(data, new TensorShape(batch, seq, total));
    }
}
