# Дополнительный ограниченный поиск: одна условно новая learning-гипотеза

Дата: 20.10.2026. Core pin `dd150aa5216e42b8a8bba14955337398ec5f8397`; planning pin `0ea8f0b0094e234ffe7734adfa95e2a9e53203f5`. Режим — только исходники, существующие документы и новый исследовательский рецепт. Runtime, сборки, обучение, production edits и protected edits не выполнялись. Документ включён в итоговое planning дополнение; runtime не выполнялся.

## Результат

**SFT-EXP-REUSE-01 — условно новая гипотеза снижения CPU/времени настоящего SFT update за счёт устранения повторного вычисления Exp.** Не заявлено улучшение качества, измеренный выигрыш или причина неудачи RP01. Статус `NEW_CONDITIONAL / SOURCE_TRACED / DESIGNED_NOT_RUN / NOT_RUNNABLE`.

Полная карточка и промпт: [SFT-EXP-REUSE-01-research-card.md](cards/SFT-EXP-REUSE-01.md). Сохранённый адресный поиск: [sft-exp-novelty-search-receipt.json](data/sft-exp-novelty-search-receipt.json). В нём все результаты четырёх запросов, пути и строки; **886 planning + 362 protected Markdown = 1248 файлов**, не только имена карточек. Это подтверждение границы поиска, а не доказательство абсолютной новизны во всех ветках и неопубликованной работе.

### Что установлено по исходнику

`ProductionSupervisedTrainingStepExecutor.Execute` передаёт native session в `Trainer.TrainSupervisedNextTokenStep`; последний вызывает native forward, CE, BackwardLoss, clipping и AdamW. Изменённые параметры читаются последующими уроками и ordinary forward readout. Это путь обучения, не внешний тест softmax.

- [Production caller:19–33](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/14.ContinuousLearning/Training/SupervisedTrainingExecution.cs#L19-L33).
- [Native SFT:102–145](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/04.TrainingLoop/Trainer.cs#L102-L145).
- [BackwardLoss:408–426](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/04.TrainingLoop/Trainer.cs#L408-L426): denominator и gradient-loop повторяют `MathF.Exp` одного logit с тем же max. Между ними logits не изменяются.

Единственная B-смена в экспериментальной source-copy — один локальный `float[V]` на вызов BackwardLoss. Первый loop записывает Exp и складывает в прежнем порядке, второй читает. Gradient/scale/objective/precision/order/optimizer не меняются. Снимается N*V повторных Exp, добавляются 4*V payload bytes на вызов плюс overhead массива. Подсчёт операций — факт исходника; уменьшение CPU/времени — гипотеза с реальным memory tradeoff. Нельзя объявить автоматически выигрыш целого update: projections могут доминировать.

### Почему это не N62-R2 или общее старое описание softmax

| Соседний механизм | Его изменение | Отличие новой гипотезы |
|---|---|---|
| N62-R2 / LF-02 | В DpoTrainer.BackwardLogProbs перенести `new float[V]` из каждой response-row в начало вызова. Exp уже сохраняется и вычисляется один раз | Здесь native SFT Trainer повторяет Exp. Предлагается создать новый local scratch, а не сокращать число существующих массивов |
| Старый `03-DPO-PAIR-AND-GRADIENT.md` | DPO signs, masks, reductions, clipping, градиентная достижимость и tiny overfit | Не содержит SFT повторного Exp, его cache или этого tradeoff; прочитан непосредственно |
| DLT-2/B2 | Точность длинных reductions/log-sum-exp в DPO | Здесь float/MathF и порядок суммы сохраняются |
| LF-03 / GRAD-ACCUM | Initial gradient copying / ownership накопления | Gradient ownership и `logits.Backward` не меняются |
| E-SFT-RESPONSE-MASK | Mask и objective SFT | Здесь objective и full-sequence mask неизменны |

N62-R2 источник: [06-new-R2-softmax-buffer.md](https://github.com/lubochka/xiigen-planning/blob/0ea8f0b0094e234ffe7734adfa95e2a9e53203f5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-update-effect-audit/06-new-R2-softmax-buffer.md). Завершённость R2: [protected registry/card:14](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L14). Его raw и полезные результаты сохраняются; повтор не назначен.

Адресный итог поиска: `BackwardLoss|exponentialSum` — четыре совпадения (caller/reachability/masking), сочетания повторного Exp/экспонент с reuse/cache/double/recompute — ноль, `expVals` — 36 совпадений с известным DPO R2 и его обсуждением. Дополнительно читались общие softmax/gradient материалы. Формальный поиск `03` рядом с softmax дал три контекстных совпадения, не отдельный SFT фактор. Гарантия содержательного прочтения всех 1248 документов не заявлена: это полный адресный поиск плюс чтение точных соседей и primary source.

### Готовность и маленький experiment recipe

Назначен короткий будущий host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SEXP01`, native Student vocab256/hidden64/1 block/q,v LoRA rank4, фиксированные data/seed/optimizer. Переносится whitelist 36 native source paths с текущего core pin; существующий старый LFT transpose patch не переносится. Код host/scaffold не создан.

Домен — узкий C# subtraction-sign, две противоположные TRAIN строки и две отдельные EXAM строки. Readout — argmax **полного** vocab для всей однобайтной response, а не выбор gold из двух разрешённых знаков. Учебные token IDs — буквально UTF-8 bytes, без выдачи byte encoding за обученный BPE. Основной A/B — по четыре native updates, один порядок TRAIN1→TRAIN2→TRAIN1→TRAIN2; всего восемь updates и 16 отдельных before/after readout forwards. Все операции, включая preparation, должны быть показаны раздельно.

**Недостающая предпосылка принадлежит автору карточки:** реальный совместимый S0 с ненулевым полным однобайтным TRAIN-ответом, явным сравнением с constant-sign1/2, полными weights/optimizer/RNG и provenance заранее ограниченной подготовки. Ни S0, ни достаточный конечный preparation budget аудит не установил. Поэтому карточка сейчас не запускается, исполнитель не ищет donor и не обучает до PASS. Чужой checkpoint без совместимого полного snapshot и provenance не назначен warm state. Результат1/2 не доказывает условного правила; полный fit2/2 не вводится как обязательный дополнительный gate для cost-фактора. EXAM не используется для выбора S0. Запрещён ложный InitialBaselineNoUpdate marker. Это не блокировка остальных memory/BPE/KNN карточек.

Карточка фиксирует default session model исполнителя, русский отчёт, точную будущую команду и каталог, один factor, claims до действия, сохранение partial, запрет replay, STOP/NEXT в карточке и полном prompt. При фактическом полезном улучшении любой метрики — успех этой оси/домена с регрессиями рядом; данный packet не содержит измеренного улучшения и не является готовым production fix.

## Reject log: новое имя не назначать

| Проверенный механизм | Причина отклонения как нового |
|---|---|
| Cosine schedule снова растёт после TotalSteps | Уже LT-N06 в planning47/05; дополнительные карты признают его известным |
| SFT warmup GetLr(0)=0 | Уже N33 и protected56 |
| Global AdamW step vs local sparse-parameter age | Уже planning53/03 mechanism coverage; нет новой отдельной native trigger цепочки в этом проходе |
| Clipping overflow / stale gradients / nonfinite ingress | Уже N33/P12/protected59 и current gradient review; не новый каталог ошибок |
| Mean keepDims=false / modulo batch broadcasting | Уже planning51/04 и protected47; кроме того нужен конкретный актуальный consumer |
| Weight overflow из finite, но аномально огромных weights | Область прежней nonfinite-проверки, legitimate caller с таким weight не установлен; не повышать до нового quality claim |
| Почти насыщенный target softmax и cancellation в p−1 | Возможная будущая numerical тема, но в этом проходе не доказан native learning trigger, не отведена от DLT/B2 и нет диагностичного начального state; отдельная карточка не назначена |
| Odd HeadDim / RoPE последний канал | Предыдущая заметка `odd-head-dim-candidate-and-dedup.md` закрыта родителем как **CLOSED_DUPLICATE**: planning50/05 и protected47 уже описывают. Не публиковать как новую находку |
| FP32 long sums, dropout, warm B0, response mask, optimizer moments, gradient locality | Известные и уже сопоставленные; не повторялись под новым ID |

## Поздняя проверка и пределы

После исходного поиска core получил commit81141d57. Root целево проверил связанные разделы обеих новых карточек, их prompts и JSON host contract: они касаются Transpose strides и serialization options, точного SFT Exp-фактора не содержат. Новая проверенная protected дельта — ещё5 Markdown. Исходники Trainer не изменены этим commit. [Поздний отчёт о восстановлении](11-latest-core-delta-and-restore.md) проверяется отдельно и не переименовывается в успех нового A/B.

[Конфликт четырёх neural-рецептов](02-neural-diagnostic-conflict.md) и [методика/skills](08-sources-and-review.md) являются отдельными разделами. Глобальная полнота всех веток и возможных пробелов не заявляется. Единственный новый экспериментальный кандидат данного дополнения — SFT-EXP-REUSE-01; он пока DESIGNED_NOT_RUN и требует адресной авторской подготовки начального состояния.

Перед публикацией проверена также вся добавленная planning дельта28Markdown на `2d1741261e9e17c0e4dec94a480b5711f5ad47d3` теми же четырьмя запросами; exact повторного SFT Exp не найден. Два чужих предложения DPO-SHIFTED-CONTEXT-01 и IDENTITY-TRAIN-INDEX-01 посвящены другим механизмам, сохранены какA1/A2. НашSFT-EXP —A3; общий проверенный корпус1281Markdown. Сохранённый JSON receipt дополнен точной дельтой, а базовые1248результатов поиска не переписаны.
