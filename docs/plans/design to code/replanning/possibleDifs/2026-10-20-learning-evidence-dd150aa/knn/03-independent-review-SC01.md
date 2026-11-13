# Независимая проверка проекта DPO-SHIFTED-CONTEXT-01

20.10.2026. Проверяющий — отдельный агент текущей сессии, не автор SC01. Проверены документы `01-DPO-SHIFTED-CONTEXT-01.md`, `02-DPO-SHIFTED-CONTEXT-01-CARD.md`, `03-SC01-EXECUTOR-PROMPT.md`, `04-SC01-FIXTURE.md` после внесённых ниже уточнений. Это документная/code review, **не native run и не подтверждение готовности host**.

**Вывод:** новое адресное различие контрактов подтверждено исходниками; проект опыта после исправления конфликтов согласован для отдельной фазы подготовки. Статус остаётся `PREPARATION_REQUIRED`, а полезный эффект не измерен. Нельзя отметить READY, restore payload, выполненный DPO или успешный последний target на основании этого review.

## Что независимо подтверждено

- В current exact DPO `CaptureAndValidate` отклоняет полный carrier T>ContextLen; полный input включает последний target, тогда как scorer/backward читают targets `ids[i+1]` только на i<T−1. [Material validation](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/11.DPO/Models/DpoExactPairMaterial.cs#L121-L137), [forward/scorer/backward](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/11.DPO/DpoTrainer.cs#L1007-L1120).
- Native SFT уже допускает carrier ContextLen+1 и раздельные L−1 inputs/targets. [Trainer.cs, строки 77–117](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/04.TrainingLoop/Trainer.cs#L77-L117). Поэтому узкая гипотеза shifted-input имеет конкретное основание, но source discrepancy не доказывает причину всех RP01 регрессий.
- Независимо разобраны literal JSONL и пересчитаны UTF-8 размеры/хеши без tokenizer/build/обучения: boundary 364 bytes, SHA256 `692be1ce627f76b459910738a9bdd98d6ffc55ecf0e9ac0f5046350cd26e60cb`; control 358 bytes, SHA256 `649df762a811b645b3fb67f35d705031f848ff205d6dec8f5e0f471714dfe048`. После decode prompt16 + response49 =65, либо response48 =64. Единственный differing byte — absolute target64/63. Это проверка текстовой спецификации, не утверждение о проверенном native BPE.
- B сохраняет полный target carrier и включает последнюю logit row, а не теряет последний target через наивный crop. Сохраняется один фактор; PAD per-side crop, response-only head, tokenizer/LR/optimizer changes сюда не добавлены.
- SFT optimizer1000 не объявлен DPO continuation; общий старт — сохранённые weights1000 и новый native DPO optimizer0. A boundary отказ и B updates оцениваются как разблокирование, не как equal-work speedup.

## Конфликты, найденные review и исправленные автором

| Проблема до review | Уточнение в финальной карточке/промпте |
|---|---|
| Все Prepare/Calibrate/Compare использовали один attempt root, но запрет существующего root блокировал последующие фазы | Root manifest создаётся один раз Prepare; следующие фазы допускают только этот точный immutable manifest и создают свои новые `calibrate`/`compare` подкаталоги. Повтор занятой фазы запрещён, старое не перезаписывается |
| Calibration budget200 не определял A/B и общий счётчик | Только variant A original full-input; 200 optimizer updates суммарно, 100 предъявлений каждой пары, B не обучается. Основной Compare снова загружает исходный warm1000, не calibration state |
| Positive anchor3/40 мог стать единственным оправданием terminal0→0 | Terminal0→0 не считается доказательством решённого смыслового подответа. Полезный NLL/probability gain всё равно сохраняется как успех своей оси с `NO_TASK_PROOF / NEEDS_CALIBRATION`. Actual0→1 допустим; 1→1+NLL gain допустим; 1→0 показан регрессией |
| Tolerance предлагалось выбрать исполнителю | Anchor count ровно3/40; state hash фиксирован. NLL/logP: abs delta ≤1e−5+1e−6×abs reference. Gradient: abs delta ≤1e−6+1e−4×max abs. Ослабление после результата запрещено |

## Что ещё не доказано и должно остаться открытым

LFS payload не восстановлен в этой сессии; native state/readout/gradient parity и достаточность именно DPO200 отсутствуют. Успех старого SFT200 не доказывает DPO200. Поэтому отдельно назначенная calibration нужна до Compare; её не выполняют скрыто, не расширяют бюджет и не ищут новую модель при неуспехе. Даже успешный control64 не предрешает boundary65.

Задача минимального fixture — смысловой ответ «1 или 2» в последнем target. Two-candidate correctness не называется full response exact; положительные первые токены donor не заменяют правильную финальную цифру. Нет вывода о свободной генерации C#, EN→RU или всей реализации RP01.

STOP/NEXT, default executor model, русский report, изолированные source copies, сохранение старых результатов и protected read-only сохранены. Новая карточка не должна возвращать исполнителя к завершённому №08: operational очередь берётся из reconciliation/`REGISTRY.md` текущего additive пакета; следующий существующий №09 — `N63-AR-CAP`, host N63R, подготовка ещё требуется.

Этот review допускает дальнейшее документирование и подготовку при соответствующем поручении. Он не разрешает автоматически запустить три фазы, исправлять production или объявлять непроведённый опыт успешным.
