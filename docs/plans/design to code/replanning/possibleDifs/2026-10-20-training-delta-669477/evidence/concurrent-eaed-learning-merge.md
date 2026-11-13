# Concurrent planning eaed: дедуп и точное объединение учебных факторов

Прочитаны две полные карточки с встроенными prompts, общий TENSOR-LEARNING-EXECUTION-CONTRACT, README, внешний entry, Markdown-реестр и структурированный реестр на planning `eaed843ad2b063fbb003f073deb69da19f0cb135`. Их7 Git blob SHA сверены. Это адресный readback нового соседнего пакета, без runtime/обучения и без утверждения о повторном полном аудите корпуса.

## Решение: сохранить оба чужих фактора, union40

Множества реестров проверены машинно:38 в опубликованном eaed,38 в нашем подготовленном пакете, пересечение36, объединение **40 уникальных ID**. Чужие два ID — GRAD-ACCUM-COPY-01 и FROZEN-BROADCAST-MAP-01; наши два дополнительных — VT-APOSTROPHE-01 и RMS-ROW-REDUCE-01. Ни одно из этих четырёх названий не является alias другого.

| ID | Точное вмешательство | Почему не повторяет другие |
|---|---|---|
| GRAD-ACCUM-COPY-01 | `TensorOps.AccumulateGrad`: убрать внешний `gradToAdd.Data.ToArray()` только при первом `target.Grad == null`; defensive copy TensorStorage остаётся | Это первый внутренний вклад gradient, а не начальный gradient Tensor.Backward/LF-03; не Mean/SumAlongAxis и не parser verifier |
| FROZEN-BROADCAST-MAP-01 | `TensorOps.MultiplyBroadcast`: не создавать `mappedBIndices` только при обоих RequiresGrad=false; coordinate loop неизменен | Это backward-map в frozen forward. RMS-COORD меняет FlatToIndices, RMS-ROW — SumAlongAxis; оба других фактора здесь прямо исключены |
| RMS-ROW-REDUCE-01 | `TensorOps.SumAlongAxis`: contiguous last-axis scalar reduction без координатных копий | Другой метод/момент вычисления; не трогает ни AccumulateGrad, ни mappedBIndices |
| VT-APOSTROPHE-01 | `QualityVerifierUnit.TryParseRequiredTerm`: полный target term внутри exact builder frame | Переводческий rule learner; ни один tensor-фактор его не меняет |

Current source подтверждён по `TensorOps:168–210,984–997,1020–1047`, `TensorStorage:21–24` и ранее проверенному translation parser. Это source-distinct, а не утверждение о measured benefit. Все4новых карточки остаются NOT_RUN. Авторство GRAD/FROZEN — **у уже опубликованного соседнего пакета**, не у этого аудита.

## Canonical ссылки, roots и entry

| Поле | GRAD-ACCUM-COPY-01 | FROZEN-BROADCAST-MAP-01 |
|---|---|---|
| Каноническая карточка | [GRAD](https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/GRAD-ACCUM-COPY-01.md) | [FROZEN](https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/FROZEN-BROADCAST-MAP-01.md) |
| Готовый prompt | В той же карточке, раздел «Готовый промпт исполнителю» | В той же карточке, тот же раздел |
| Физический workspace | `C:/xg/AGC1` | `C:/xg/FBM1` |
| Source / host | `src/` и `host/` внутри workspace | `src/` и `host/` внутри workspace |
| Единственный entry | `pwsh -NoProfile -File 'C:/xg/AGC1/host/Run.ps1'` | `pwsh -NoProfile -File 'C:/xg/FBM1/host/Run.ps1'` |
| Claim | `C:/xg/AGC1/host/execution.claim` | `C:/xg/FBM1/host/execution.claim` |
| Results / report / candidate | `C:/xg/AGC1/results/`; `report.md`, `candidate.md` | `C:/xg/FBM1/results/`; `report.md`, `candidate.md` |

Обязательная часть обеих карточек — [TENSOR-LEARNING-EXECUTION-CONTRACT](https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/TENSOR-LEARNING-EXECUTION-CONTRACT.md). Четыре полные native source-copy библиотеки + bridge; source pin883496 сохраняется. Настоящий Student: rp01-csharp-small/v1, q/v-LoRA, одна simulation-пара, один StepVector на плечо; на один опыт2optimizer updates суммарно и4DIAG_TRAIN forwards отдельно. Host не создан и не собран. Наличие local B0/binary dependencies в этом readback не проверялось.

Логические project paths `35.ExperimentsV2/Experiments/Rp01Micro/AGC1|FBM1` **не создают вторую физическую копию**; общий договор явно назначает короткие workspaces. Не заменять их нашими RD01/QVA01 и не переносить результаты в protected. Canonical [README](https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/README.md), [реестр](https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/04-execution-registry.md), [JSON](https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/experiments/concurrent-merged-registry.json). Все точные URL и поля для merge доступны в соседнем `concurrent-eaed-learning-merge.json`.

## Минимальное согласование очереди без изменения чужих научных рецептов

Опубликованный относительный порядок — **GRAD35 → FROZEN36**. Рекомендуем сохранить его: **BPE-REALIZED34 → GRAD35 → FROZEN36 → VT37 → RMS38 → IDENTITY39 → KN40 → END**. Это сохраняет оба уже опубликованных порядка и ставит все4direct learners перед dependent readers.

В новом реестре/явном execution overlay потребуются только новые эффективные назначения:

- FROZEN NEXT теперь VT-APOSTROPHE-01; его старое NEXT=IDENTITY остаётся историческим назначением соседнего пакета.
- VT NEXT=RMS, RMS NEXT=IDENTITY; GRAD NEXT=FROZEN сохраняется.
- Старые38/6completed/32pending и общий NEXT=LF-03 относятся к source827. После проверки седьмого LF-03 результата на6e3e свежий статус должен стать40/7completed/33pending и NEXT=N63-INGRESS-LENGTH. Статус LF-03 определяет coverage-agent по primary receipts, а не этот source-only readback.
- Встроенные prompts и общий tensor-договор читать вместе с новым явным overlay: старое глобальное NEXT не разрешает повтор завершённого опыта. Source pin, один фактор, roots, вход, бюджет и NOT_RUN новых факторов не меняются.

Полный повтор внешних карточек под нашим авторством или их runtime-исполнение для объединения не нужны. Условный HIST-E07-GLOBAL-01 соседнего README остаётся вне активной очереди: доказанный накопительный caller не предъявлен; это не41-й запуск.

## Production после нового core6e3e

Сравнены корневые Git trees827 и `6e3e8864933e35fccdb4f21369c9912fa195aaf6` (оба truncated=false). **Все45 production project trees совпали**; исключены34.Experiments,35.ExperimentsV2,36.LibraryComparison. Изменились только root entries35.ExperimentsV2,docs,experiments-v2. Следовательно TensorOps/TensorStorage, RMS36 и Translation7 сохранили bytes. Квитанция — `core6e3-production-tree-delta.json`. Это не оценка нового LF-03 результата и не runtime parity.

