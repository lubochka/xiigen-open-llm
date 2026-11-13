using System.Text.Json;
using TensorCore.Core;
using TensorCore.Layers;
using TransformerModel.Config;
using TransformerModel.Layers;

namespace TransformerModel;

// Полная decoder-only языковая модель:
//   token_embedding → N × TransformerBlock → RmsNorm → lm_head
// lm_head проецирует из HiddenDim в VocabSize для получения логитов следующего токена.
public class LlmModel : Module
{
    private readonly Embedding           _tokenEmbedding;
    private readonly List<TransformerBlock> _blocks;
    private readonly RmsNorm             _finalNorm;
    private readonly Linear              _lmHead;
    private readonly ModelConfig         _cfg;

    public ModelConfig Config => _cfg;
    public IReadOnlyList<TransformerBlock> Blocks => _blocks;

    public LlmModel(ModelConfig cfg)
    {
        _cfg = cfg;

        _tokenEmbedding = new Embedding(cfg.VocabSize, cfg.HiddenDim);
        _blocks = Enumerable.Range(0, cfg.NumLayers)
            .Select(_ => new TransformerBlock(cfg))
            .ToList();
        _finalNorm = new RmsNorm(cfg.HiddenDim, cfg.RmsNormEps);
        _lmHead    = new Linear(cfg.HiddenDim, cfg.VocabSize, useBias: false);

        RegisterModule("token_embedding", _tokenEmbedding);
        for (int i = 0; i < _blocks.Count; i++)
            RegisterModule($"block_{i}", _blocks[i]);
        RegisterModule("final_norm", _finalNorm);
        RegisterModule("lm_head",   _lmHead);
    }

    // Прямой проход: tokenIds [B, T] → logits [B, T, VocabSize]
    public Tensor Forward(Tensor tokenIds)
    {
        var x = _tokenEmbedding.Forward(tokenIds);
        foreach (var block in _blocks)
            x = block.Forward(x);
        x = _finalNorm.Forward(x);
        return _lmHead.Forward(x);
    }

    // Скрытые состояния до lm_head (для генерации эмбеддингов)
    public Tensor ForwardHidden(Tensor tokenIds)
    {
        var x = _tokenEmbedding.Forward(tokenIds);
        foreach (var block in _blocks)
            x = block.Forward(x);
        return _finalNorm.Forward(x);
    }

    // Атомарное сохранение весов (DNA rule 7: write .tmp, затем rename)
    public void SaveCheckpoint(string path)
    {
        var tmp = path + ".tmp";
        SaveWeights(tmp);
        File.Move(tmp, path, overwrite: true);
    }

    public void LoadCheckpoint(string path) => LoadWeights(path);

    // Сериализация: сохраняем все параметры как binary float[]
    private void SaveWeights(string path)
    {
        using var fs = File.OpenWrite(path);
        using var bw = new BinaryWriter(fs);

        var allParams = Parameters().ToList();
        bw.Write(allParams.Count);

        foreach (var p in allParams)
        {
            bw.Write(p.Value.Shape.Rank);
            foreach (var d in p.Value.Shape.Dimensions)
                bw.Write(d);
            foreach (var v in p.Value.Data)
                bw.Write(v);
        }
    }

    // Десериализация: загружаем параметры в том же порядке
    private void LoadWeights(string path)
    {
        using var fs = File.OpenRead(path);
        using var br = new BinaryReader(fs);

        var allParams = Parameters().ToList();
        int count = br.ReadInt32();

        for (int pi = 0; pi < Math.Min(count, allParams.Count); pi++)
        {
            int rank = br.ReadInt32();
            var dims = new int[rank];
            for (int i = 0; i < rank; i++) dims[i] = br.ReadInt32();

            var shape = new TensorShape(dims);
            for (int i = 0; i < shape.TotalElements; i++)
                allParams[pi].Value.Data[i] = br.ReadSingle();
        }
    }
}
