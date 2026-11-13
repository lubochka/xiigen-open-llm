# N27-A — E48: скрытый выигрыш при малом числе примеров и отсутствие настоящего checkpoint transfer

<!-- XIIGEN-AUDIT-2026-11-11-INSERT -->
<!-- AUDIT-2026-11-11-EV-P49:START -->
## Уточнение перекрытий P49 — 11 ноября 2026

Для последующего P49 attempt3 на core `009bf226174441fa0fbf0336e74ad2f2b7174d92` независимо пересчитано: cold EXAM содержит2 exact-pair replay,1 знакомый input с новой целью и1 действительно новый input; warm —4 exact-pair replay. Cold e-t1=0→1 использует знакомый0, уже предъявленный с целью3. Поэтому «не встречалась пара» не означает «не встречался вход». Существующий контроль1→5 также имеет знакомый input1 и должен проверять новую цель, а не unseen input.

В требуемую ниже overlap-таблицу добавить эти три взаимоисключающие категории для истории каждого плеча; изменение только gold не должно менять input-novelty count. Выигрыш нового материала7/8 против6/8, retention6/6→3/6 и цена warm600+60 против cold60 сохраняются. Finite gate уже исправлен в current attempt3; новый native запуск не выполнялся. Исправление терминов и negative oracle описано в [дополнении11.11](2026-11-11-P48-P58-EXPERIMENT-MEASUREMENT-ADDENDUM.md); пересчёт не требует повторного обучения.
<!-- AUDIT-2026-11-11-EV-P49:END -->


<!-- AUDIT-2026-11-09-3161:START -->
## Граница последующих выводов о тёплом старте — 9 ноября 2026

Ниже остаётся исходный разбор E48 на его срезе. Последующий [P49](https://github.com/lubochka/llm_mvp_core/blob/31612135c97e15be64e1b2a4a21efd3ffd58e443/docs/plans/replanning-possible-diffs/experimets/cards/P49-probe-warm-start.md) уже имеет сохранённую проверку исправленной производной: на рассмотренной панели64/64 actual совпали с прежними, new material cold6/8 иwarm7/8. Польза+1/8 сохраняется для этой дозы, но4/4 EXAM-входа повторяют TRAIN в представлении модели, а старые учебные строки warm6/6→3/6. Название EXAM и отдельные file IDs не создают независимость данных. Учесть600 подготовительных updates; один endpoint не доказывает скорость обучения.

При дальнейшей передаче карточки потребовать таблицу перекрытий по model-visible input и input-target pair, раздельные retained/gained/lost для всего прежнего TRAIN и нового материала, стоимость подготовки и продолжения, а также фактический typed checkpoint. Поднабор `old_only`4/4 не заменяет удержание всех старых6 строк. Само наличие loss/hash не удостоверяет state, learning или обобщение.

Перед будущим измерением loss проверить его согласованность с производной по [C04](https://github.com/lubochka/llm_mvp_core/blob/31612135c97e15be64e1b2a4a21efd3ffd58e443/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/105-addressed-repair-plan-2026-10-30.md), включая насыщенные вероятности. Это новый узкий диагностический остаток; уже подтверждённые actual/accuracy и правильный backward не отменяются автоматически. Весь пересчёт сохранённых таблиц выполняется без Train; новое независимое EXAM и кривая доз остаются отдельным исследованием с собственным поручением. Данные, команды и бюджеты исторических плеч ниже не меняются.
<!-- AUDIT-2026-11-09-3161:END -->

**Дата:** 2026-10-16. **Тип:** новый адресный source/code+archive audit поверх DIF-01/05/31/40. **Статус:** SOURCE_CONFIRMED / ARCHIVED_METRIC_CONFIRMED / CAUSAL_RP01_NOT_RUN / INDEPENDENT_CODEX_REVIEW_NOT_RUN. **Запись только в xiigen-planning/llm_plan3/possibleDifs.** Каталог llm_mvp_core/docs/plans/replanning-possible-diffs неизменен.

## 1. Вывод для человека

Старый опыт E48 получил FAIL по конечной точности, хотя на промежуточном размере набора warm-start оказался заметно лучше cold-start. При этом код НЕ проверял, что сохранённое, уже обученное состояние загружается для нового домена: он обучает новые объекты заново. Таким образом нельзя делать ни вывод «перенос не работает», ни вывод «RP01 исправится от warm start». Но этот опыт представляет самостоятельный кандидат на причину плохого обучения RP01 при малом бюджете и случайной B0.

## 2. Проверяемые первичные свидетельства

1. [E48DomainBootstrapper.cs, строки 8–39](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E48DomainBootstrapper.cs#L8-L39), blob SHA d117849405be4482f9aaba10637b8f3f6c971474: описание обещает warm start from nearest domain; фактически source = первые 60% переданного списка файлов, target = остальные 40%, без поиска ближайшего домена. Target TRAIN и VAL выделяются последовательным разрезом extracted predicates, не независимыми source-families. Это конкретные свойства экспериментального кода; bias исходного порядка и leakage нужно проверять отдельно.
2. [E48DomainBootstrapper.cs, строки 42–79](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E48DomainBootstrapper.cs#L42-L79): заранее созданный warmSrcNet обучается на source с learning rate 0.01, но затем не передаётся, не сохраняется и не восстанавливается в enhanced arm. Для каждого training-budget checkpoint создаётся НОВЫЙ объект c learning rate 0.02, на нём повторяют source updates и затем target updates. Это re-training imitation, а не доказанный persistence/restore transfer. Cost source updates включён только в enhanced.
3. [E48DomainBootstrapper.cs, строки 80–110](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E48DomainBootstrapper.cs#L80-L110): 25%-эффективность считается как secondary ScalingSignal, но primary BaselinePrimary/EnhancedPrimary берутся только на последнем checkpoint. Общий вердикт может быть FAIL при улучшении на меньшем бюджете.
4. [Сохранённый E48 run 2026-04-30](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E48-DOMAIN-BOOTSTRAPPER/results/run-2026-04-30-001.json) и [2026-05-01](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E48-DOMAIN-BOOTSTRAPPER/results/run-2026-05-01-001.json), последний blob 7755ba5dbd6b984020b433150e5e6b92e26ea029: оба записали Tier2 final cold=.3036, warm=.3036, passed=false, одновременно notes cold=[N22=14%, N56=14%, N112=30%, N224=30%] против warm=[N22=14%, N56=34%, N112=34%, N224=30%]. При N56 абсолютная разница по округлённым процентам +20 пунктов; соотношение 34%/14% — НЕ статистическое доказательство. Валидировано 57 extracted predicates, НЕ 57 независимых доменных задач. Эти два повторённых архива с одинаковыми данными/тренировочной последовательностью не считать двумя независимыми экспериментами.
5. [RP01 Learning Recovery](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md): у нынешнего RP01 случайная непредобученная B0, 50 обновлений C#500, затем 50 ENRU500, а свободное качество на узких панелях остаётся нулевым. **Механизм E48 НЕ используется автоматически в RP01 Student.**

## 3. НОВЫЕ различимые гипотезы

**N27-A1 — потеря низкобюджетной learning-sample-efficiency.** Текущая оценка по финальной эпохе игнорирует важную для PC и 50-step budget точку быстрого освоения. Предсказание: при одинаковом суммарном computation более ранний checkpoint реального transfer-arm имеет преимущество на source-disjoint holdout, а поздний сливается с cold arm. Опровержение: на честном paired holdout преимущества нет уже на ранней точке.

**N27-A2 — source-pretrain recompute вместо state handoff.** Старый эксперимент не моделировал persist→load→fine-tune, следовательно не доказывал важную для RP01 функцию корректного переноса модели/optimizer/RNG. Предсказание: persistent warm checkpoint и freshly re-trained warm model дадут разные значения, если restore/state identity or optimizer continuity нарушена. Опровержение: exact params/logits/optimizer+RNG после cold restore совпадают в пределах объявленной точности и обучение детерминированно воспроизводится.

**N27-A3 — «nearest» donor выбирался не nearest, а порядком источников.** Представление и source-family selector могут влиять сильнее самого обучения. Предсказание: при order reversal и равном бюджете эффект E48 может измениться; настоящая nearest-via-verified-tags версия должна быть устойчивее, чем first60%. Опровержение: source selection и order не влияют на сопоставимую законную тестовую панель.

**N27-A4 — модель и экспериментальная единица несовместимы.** E48 использует ProbeNeuralNetwork из 22 predicate IDs, RP01 — Transformer/LoRA для 629 BPE IDs. Статистику E48 нельзя переносить на способность генерировать русский или C# текст. Это возможная ошибка интерпретации, а не подтверждённый neural defect.

## 4. Один короткий различающий опыт

Следовать [N27 микро-протоколу](27-MICRO-LOW-BUDGET-TRANSFER-AND-GRAPH-CLOSURE-2026-10-16.md), блок E48: один pinned source state, 8–16 approved TRAIN и 12 независимых heldout cases (по source families), три arm: A fresh base + small target; B same base, source pretrain, **сохранить/перезагрузить реальные веса**, затем small target; C B с shuffled/source-mismatched donor (отрицательный контроль). Все 3 target budgets, одна версия tokenizer и frozen exam, paired Q_law и critical errors; training source steps учитывать отдельно по wall-clock и updates. Различать learned outcome от retrieval copy. P0 на warm checkpoint restore equality до quality run; stop на invalid artifact. Повторить с минимум двумя seed только если бюджет и source split допускают интерпретацию. Native RUN в текущем исследовании НЕ выполнялся.

## 5. Code/review verdict

**Источник подтверждает разрыв контракта исследования**, но не доказал ухудшение RP01 от N27-A1/2/3. Исправлять существующий RP01/production код и переименовывать исторический FAIL до независимого CODEX/Astra 6 UltraCode выбора и quality-assessment запрещено. Не модифицировать E48 original output или рубрики. Архитектурное следствие только после контраста на том же ordinary consumer.
