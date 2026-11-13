# Кандидат DPO-MASK8: корректный градиент дробных масок в живом DPO

## Решение и область действия

**Зафиксировать `dpo-mask-live8-01` как успешное исправление полезного учебного показателя: корректности weighted-mask производной, которая действительно меняет optimizer moments и LoRA policy при настоящих DPO updates.** Успех по этой оси не требует роста точности. Правильных переводов осталось 0/24, средний online DPO loss немного вырос, а часть расходов ухудшилась. Все изменения показаны ниже. [Первичный отчёт][report]

В текущем core SHA `9622b575a6aabb79939b12cc9f5d96313f1bf370` исправление уже существует: `BackwardLogProbs` использует сумму масок в знаменателе legacy mean и умножает вклад строки на значение маски. Статус кандидата — `SUCCESS_RECORDED / SOURCE_ALREADY_PRESENT`; повторный production patch не требуется. [Текущий оригинал:1072–1121][backward]

**Предметная область:** перевод. **Профиль:** EN→RU, legacy-full-sequence-mean-v1 carrier с дробной маской первого response target 0.25, 16 q/v-LoRA tensors, frozen base/reference. Это не стандартный exact-v2 RP01: у него бинарная response mask. Именно этот предел запрещает объявлять исправленный дробный legacy gradient установленной причиной провала обычного exact RP01. Бинарные legacy и exact-sum производные до/после в сохранённой проверке равны. [Report:9–20][report]; [validation][validation]

Это добавочная запись результата уже проведённого 2026-10-18 опыта. Защищённые источники не меняются; новых runtime экспериментов или продуктовых правок этот аудит не выполнял.

## Почему это действительный учебный опыт

Harness загружает две независимые копии C50: policy и reference. Reference целиком заморожена, у policy обучаются 16 LoRA tensors при 39 frozen base tensors. Governance, projection содержания, native pair preparation, pair customs, translation lease и mutation capability используются в исходной цепи. У первой response позиции chosen и rejected единственное изменение входного режима — mask 0.25; prompt/padding остаются 0, остальные response targets — 1. Этот режим одинаков в A/B. Затем восемь раз вызван настоящий public `DpoTrainer.Step`, с native backward, optimizer и checkpoint publication. [Harness:49–84][harness]

Фактор между A/B — только производная weighted log probability: знаменатель равен сумме масок, и каждый token contribution умножается на свою маску. Формула forward score не менялась. На каждом из восьми шагов различаются все 16 first-moment keys между плечами; итоговые policy weights различаются, base/reference hashes сохраняются. Следовательно, исправление вошло в реальное обучение. Само различие hashes не является доказательством правильного перевода; корректность производной проверена дополнительно численным guard и независимым float64 oracle. [Validation][validation]; [Report:11–22][report]

Оригинальная цепь: `DpoTrainer.Step` → policy/ref `ComputeTargetLogProbs` → preference objective → `BackwardLogProbs` → `logits.Backward` → LoRA gradients → AdamW → публикация policy. [Step:650–697][step]; [score/backward:1036–1121][score]

## Сопоставимость и точные определения

| Поле | A и B |
|---|---|
| Исходный checkpoint | Тот же C50 SHA256 `6f4d0284ea823ba99932b0f28576855c14205703115b48c983c7a9f03749179b` |
| Старт | Равны policy/reference/base hashes, RNG, tokenizer; optimizer step 0 |
| Учебные данные | Те же 8 пар в одном порядке; material JSON побайтно одинаков |
| Топология | По 4 блока hidden512 на policy/reference; 16 trainable policy tensors, 39 frozen base; reference целиком frozen |
| Native objective | Legacy weighted mean log probabilities; βDPO=.1; один `Step` на пару |
| AdamW | LR=1e−5, β₁=.9, β₂=.95, ε=1e−8, weightDecay=.01 |
| Стохастика | Dropout=0; начальные RNG одинаковы |
| Оценка | 24 одинаковых generation входа; overlap TRAIN/EVAL=0; собственный native TextGenerator |
| Изменённая assembly | Только DPO; harness и остальной source/binaries зафиксированы |
| Внешние teacher calls | 0 |

Источники: [initial state][initial], [validation][validation], [training material][material]. Аудит дополнительно пересчитал сохранённые 8 строк и проверил их порядок и побайтное равенство учебного материала. Это чтение и арифметика над receipts, не новый обучающий запуск.

Для одного chosen/rejected текста legacy score равен `Σ(mask[t] × logp[t]) / Σ(mask[t])`. Знаменатель суммирует response targets; prompt/padding нулевые. Значения ниже — весовые суммы для двух сторон, одинаковые в A/B. `ChosenLength` в raw включает prompt, поэтому не следует путать его с числом только response tokens.

| Шаг / pair ID | Σchosen mask | Σrejected mask | Pre-update loss A | Pre-update loss B |
|---|---:|---:|---:|---:|
| 1 / enru-0385 | 68.25 | 56.25 | 0.69314720 | 0.69314720 |
| 2 / enru-0111 | 74.25 | 54.25 | 0.69314384 | 0.69314384 |
| 3 / enru-0323 | 71.25 | 78.25 | 0.69314647 | 0.69314635 |
| 4 / enru-0168 | 84.25 | 71.25 | 0.69315030 | 0.69315050 |
| 5 / enru-0047 | 68.25 | 74.25 | 0.69315480 | 0.69315460 |
| 6 / enru-0376 | 43.25 | 38.25 | 0.69314100 | 0.69314134 |
| 7 / enru-0269 | 43.25 | 43.25 | 0.69313930 | 0.69314003 |
| 8 / enru-0098 | 46.25 | 49.25 | 0.69315980 | 0.69315970 |
| Сумма восьми loss | — | — | 5.54518271 | 5.54518356 |
| Среднее: сумма / 8 пар | — | — | **0.69314783875** | **0.69314794500** |

Это среднее восьми native `DpoMetrics.Loss`, вычисленных **до update каждого шага**, при последовательном обучении на разных, но попарно одинаковых A/B примерах. Оно сопоставимо по данным, objective, масштабу и числу updates. Оно **не является loss финальных моделей на общей отложенной панели**. Сравниваемая величина ухудшилась на **+0.00000010625**; округление до 0.693 в исходном отчёте скрывало эту небольшую разницу, а не измеренное улучшение. [A steps][steps-a]; [B steps][steps-b]; [native Step:668–691][step]

## Результаты и знаменатели

| Показатель | A | B | Значение результата |
|---|---:|---:|---|
| Native finite difference, прошедшие строки / 162 | 126/162 | 162/162 | **Успех корректности учебной производной** |
| Float64 oracle, совпавшие координаты / 54 | 42/54 | 54/54 | **Успех корректности**, независимая численная проверка |
| Изменённые LoRA tensors / 16 | 16/16 | 16/16 | Реальное обучение в обоих плечах |
| Изменённые frozen base tensors / 39 | 0/39 | 0/39 | Frozen contract сохранён |
| First-moment keys, различающиеся между плечами | — | 16/16 на каждом из 8 шагов | Фактор достиг optimizer, не ограничился guard |
| Итоговые policy weights между плечами | — | Различаются | Подтверждает действие фактора, не качество ответа |
| Правильные собственные ответы / 24 | 0/24 | 0/24 | Нет роста точности |
| Gained / lost answer IDs | 0 / 0 | 0 / 0 | Изменения правильных ответов нет |
| Совпадение generated text A/B | — | 24/24 | Итоговые ответы одинаковы |
| Online preference accuracy / 8 учебных пар | 4/8 | 4/8 | Не путать с 0/24 translation accuracy |

Native guard — **6 сценариев × 9 logits coordinates × 3 размера конечной разности = 162 строки**. Float64 oracle — **6 × 9 = 54 координаты**. Это не 162 и 54 независимых обучающих опыта. Сценарии: fractional .25/.75, binary legacy, binary exact-sum, zero-token, all-zero. Все обучающие пары несут по одному fractional target с каждой стороны. [Harness:95–105][guard]; [validation][validation]

Для полноты прочитаны и численные preference metrics из тех же восьми `DpoMetrics`:

| Среднее по 8 online шагам | A | B | B−A |
|---|---:|---:|---:|
| ChosenReward | −0.0000030100345625 | −0.0000019788740275 | +0.000001031160535 |
| RejectedReward | −0.0000016927718875 | −0.00000044107450375 | +0.00000125169738375 |
| RewardMargin | −0.0000013172627375 | −0.0000015377997875 | −0.00000022053705 |
| Loss | 0.69314783875 | 0.693147945 | +0.00000010625 |

Рост chosen reward является полезным локальным направлением отдельного сигнала относительно reference. Одновременно rejected reward вырос сильнее, поэтому margin и loss ухудшились. Ни одна из этих online preference метрик не названа улучшением собственных переводов. Источники — [A/B шаги][steps-a] и [after][steps-b].

## Расходы и их область измерения

| Метрика | A | B | B−A |
|---|---:|---:|---:|
| Сумма 8 native Step с публикацией, мс | 36 113.9900 | 34 532.0568 | **−1 581.9332** |
| Полный harness, мс | 155 791.2765 | 159 018.7596 | +3 227.4831 |
| Генерация 24 ответов, мс | 112 050.1296 | 118 468.4103 | +6 418.2807 |
| CPU всего процесса, мс | 154 890.625 | 157 968.750 | +3 078.125 |
| Peak working set процесса, байт | 1 958 367 232 | 2 199 101 440 | +240 734 208 |
| Максимальная наблюдённая сумма RAM группы, байт | 1 965 518 848 | 2 181 369 856 | +215 851 008 |
| Cumulative managed allocations, байт | 184 322 779 688 | 184 322 688 336 | **−91 352** |
| Восемь native публикаций, байт | 383 197 151 | 383 197 154 | +3 |
| Один trained checkpoint, байт | 47 899 519 | 47 899 519 | 0 |

Источники: [A cost][cost-a], [B cost][cost-b], [первичный отчёт][report]. Разница публикаций +3 относится к сумме файлов/метрик, не изменению размера матриц checkpoint.

Таймер шага стоит непосредственно вокруг `trainer.Step`, после issued-step receipt. Он включает native forward/backward/optimizer/checkpoint publication, но не последующий экспорт optimizer для audit-файла. Поэтому это сравнение полного native шага, не отдельно чистого backward/optimizer. [Harness:78–81][harness]

Снижение суммы времени восьми шагов — локальный успех измеренного показателя; −91 352 cumulative allocated bytes — локальный выигрыш по общепроцессному выделению памяти. Вторую величину нельзя выдавать за training-only allocations или снижение peak RAM: peak, напротив, вырос. Один запуск на плечо, JIT и порядок запуска ограничивают причинный вывод о скорости. Это не отменяет установленный успех учебной производной. Времена полного harness, генерации, CPU и память не скрываются.

## Запись в кандидаты исправлений

| Поле | Значение |
|---|---|
| Candidate ID | DPO-MASK8-RETAIN |
| Измеренный опыт | `dpo-mask-live8-01`, 2026-10-18 |
| Где чинить оригинал | `11.DPO/DpoTrainer.cs`, `BackwardLogProbs` |
| Как чинить | Для legacy mean делить upstream на сумму mask; каждый token gradient умножать на его mask, сохраняя sum objective |
| Текущее состояние | Исправление уже находится в pinned current source |
| Признанный успех | Корректная дробная weighted-mask производная, достигающая реальных 16 LoRA optimizer states; локальные cost gains отдельно |
| Не доказано | Улучшение точности перевода, преимущество на обычном бинарном exact RP01, восстановление всей цепи RP01, общий speedup |
| Новый product patch | Не назначен |
| Новый runtime запуск | Не назначен; это documentation-only record |
| Архив воспроизведения | [`commands.md`][commands] с DpoLive/Rp01SimulationDpo build и before/after receipt-командами |

Архивный harness использовал настоящую friend-assembly и владельца допуска; эта запись не поручает подменять capability или запускать эксперимент через произвольную assembly. Исходные команды содержат исторические локальные Windows paths; они являются источником выполненного запуска, а не готовой переносимой командой для новой машины.

**Промпт документирующему исполнителю:** «Добавь результат как успешный кандидат сохранения уже применённого gradient fix. Любое измеренное улучшение полезного показателя считай успехом по этому показателю и данному профилю; роста accuracy не требуй. Покажи рядом 0/24, 4/8 online preference accuracy, небольшое ухудшение online loss/margin и выросшие ресурсы. Сохрани fractional legacy scope и binary exact counterevidence. Не подменяй actual DPO диагностикой logits; опирайся на уже записанные восемь обновлений LoRA. Новое обучение, product patch и изменение защищённых карточек не выполняй».

[report]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/report.md#L3-L37
[harness]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/harness/Program.cs#L49-L91
[guard]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/harness/Program.cs#L95-L105
[backward]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/11.DPO/DpoTrainer.cs#L1072-L1121
[step]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/11.DPO/DpoTrainer.cs#L650-L697
[score]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/11.DPO/DpoTrainer.cs#L1036-L1121
[initial]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/before/initial-state.json
[material]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/before/training-material.json
[validation]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/validation.json
[steps-a]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/before/training-steps.json
[steps-b]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/after/training-steps.json
[cost-a]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/before/cost.json
[cost-b]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/after/cost.json
[commands]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/commands.md


## Оговорка о хешах локального аудита

В companion `positive-neural-candidates.json` поле `hash_scope=local_materialized_download` означает SHA256 полученной локальной текстовой копии. Download/apply_patch мог добавить terminal LF или нормализовать переводы строк; raw Git blob identity по этим hashes не заявляется. Версию задают immutable source URLs. Побайтное равенство учебного материала во время выполненного A/B подтверждается первичным `validation.json`; локальный аудит дополнительно проверил полученные копии и порядок/содержимое token IDs. Это не новая runtime-проверка.
