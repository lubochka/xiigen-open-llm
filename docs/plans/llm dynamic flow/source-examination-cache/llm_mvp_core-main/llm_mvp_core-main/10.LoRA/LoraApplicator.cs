using TensorCore.Layers;
using TransformerModel;

namespace LoRA;

// Подготавливает модель к LoRA fine-tuning:
//   1. Замораживает все базовые параметры.
//   2. Возвращает LoraLinear адаптеры для каждого подходящего слоя внимания.
// Forward pass базовой модели не меняется; адаптеры применяются снаружи.
public static class LoraApplicator
{
    public static IReadOnlyList<LoraLinear> Apply(LlmModel model, LoraConfig config)
    {
        // Заморозить все базовые параметры
        foreach (var p in model.Parameters())
            p.Value.RequiresGrad = false;

        var adapters = new List<LoraLinear>();

        foreach (var block in model.Blocks)
        {
            var attn = block.Attention;

            // Создаём LoRA адаптер для каждого Linear подмодуля внимания
            // чьё имя совпадает с целевыми модулями в config
            foreach (var (name, subModule) in GetNamedLinears(attn))
            {
                // Нормализуем имя поля (_qProj) и имя цели (q_proj): убираем символы подчёркивания
                // и сравниваем без учёта регистра, чтобы "_qProj" совпало с "q_proj"
                string normName = name.Replace("_", "").ToLowerInvariant();
                bool isTarget = config.TargetModules.Any(t =>
                    normName.Contains(t.Replace("_", "").ToLowerInvariant()));
                if (!isTarget) continue;

                adapters.Add(new LoraLinear(subModule, config));
            }
        }

        return adapters;
    }

    // Возвращает все Linear подмодули с именами через рефлексию по соглашению
    private static IEnumerable<(string name, Linear linear)> GetNamedLinears(Module parent)
    {
        // Используем рефлексию чтобы найти поля типа Linear
        var type = parent.GetType();
        foreach (var field in type.GetFields(
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public))
        {
            if (field.GetValue(parent) is Linear linear)
                yield return (field.Name, linear);
        }
    }
}
