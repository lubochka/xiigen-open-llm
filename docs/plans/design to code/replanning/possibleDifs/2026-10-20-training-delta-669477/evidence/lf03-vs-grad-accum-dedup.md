# Узкая сверка LF-03 и GRAD-ACCUM-COPY-01

**Решение: разные A/B-факторы. GRAD-ACCUM-COPY-01 не получает `ALIAS_COVERED_NO_RERUN`.** LF-03 остаётся завершённым и не повторяется; GRAD остаётся будущим опытом. При остальных неизменных строках объединённого реестра: **40 IDs, 7 завершённых, 33 будущих**.

Проверено только сохранённое изменение LF-03 в core `6e3e8864933e35fccdb4f21369c9912fa195aaf6`, его source manifest и canonical GRAD card в planning `eaed843ad2b063fbb003f073deb69da19f0cb135`. Обучение и сборка не запускались.

| Признак | Выполненный LF-03 | Canonical GRAD-ACCUM-COPY-01 |
|---|---|---|
| Метод | `Tensor.Backward(Tensor initialGradient)` | `TensorOps.AccumulateGrad(Tensor target, Tensor gradToAdd)` |
| Исходный файл | `00.TensorCore/Core/Tensor.cs` | `00.TensorCore/Core/TensorOps.cs` |
| Фактически сохранённая A-строка | `Grad = new Tensor(initialGradient.Data.ToArray(), initialGradient.Shape);` — LF-03/A/Core/Tensor.cs:99 | `target.Grad = new Tensor(gradToAdd.Data.ToArray(), gradToAdd.Shape);` — TensorOps.cs:990 |
| B-строка | `Grad = new Tensor(initialGradient.Data, initialGradient.Shape);` — LF-03/B/Core/Tensor.cs:103 | `target.Grad = new Tensor(gradToAdd.Data, gradToAdd.Shape);` — заданный diff карточки |
| Когда выполняется | Однократная установка начального градиента на tensor, на котором вызван `Backward`, **до** `TopologicalSort()`/вызова backward-функций | При первом вкладе в достигнутый target с `RequiresGrad=true` и `target.Grad==null`, **внутри** backward-функций операций |
| Защитная копия | Копия TensorStorage сохранена | Копия TensorStorage должна остаться; `Tensor.Backward(initialGradient)` сохранён без LF-03 изменения |

В сравнении сохранённых A/B `Tensor.cs` единственное функциональное изменение — указанная строка `Backward`; остальные добавленные строки — комментарий фактора. В этом классе нет метода `AccumulateGrad`. Поэтому формулировку «LF-03 меняет Tensor.AccumulateGrad» следует исправить на **«LF-03 меняет Tensor.Backward(Tensor initialGradient)»**.

LF-03 `source-manifest.json` указывает `armFiles.TensorCore=["Core/Tensor.cs"]`. Для `Core/TensorOps.cs` он содержит `fromArm=false`, одинаковые `sourceSha256` и `copySha256` `e46bc0a9e6089b2712691c23beb9c5e5025fbf17a227b79924efd06890af4899`. Следовательно, сохранённая архитектура этого опыта использует общий TensorOps; отдельный B-diff `AccumulateGrad` не задан. Canonical GRAD, напротив, прямо запрещает присоединять LF-03 initial-gradient изменение и сохраняет исходный `Tensor.Backward(initialGradient)`.

Это устанавливает различие **исходников/интервенций**, а не устраняет ранее описанный разрыв исторической аттестации бинарника LF-03: `bSha256="APPLIED_AFTER_FACTOR_EDIT"` остаётся placeholder в его manifest. Успех/дельты LF-03 нельзя переносить на GRAD; его ресурсный эффект ещё не измерен.

## Точные источники

- [LF-03 A, Tensor.cs:94–102](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/35.ExperimentsV2/Experiments/Rp01Micro/LF-03/A/Core/Tensor.cs#L94-L102).
- [LF-03 B, Tensor.cs:94–106](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/35.ExperimentsV2/Experiments/Rp01Micro/LF-03/B/Core/Tensor.cs#L94-L106).
- [LF-03 source manifest](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/35.ExperimentsV2/Experiments/Rp01Micro/LF-03/source-manifest.json).
- [Canonical GRAD-ACCUM-COPY-01](https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/GRAD-ACCUM-COPY-01.md), разделы «Единственный A/B-фактор», «Отличие от прежних карточек». Авторство карточки — concurrent пакет, не настоящий аудит.
- [TensorOps.AccumulateGrad:984–997, исходный pin canonical GRAD](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/00.TensorCore/Core/TensorOps.cs#L984-L997).

