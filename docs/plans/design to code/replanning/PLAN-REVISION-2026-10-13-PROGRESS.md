# Публикация результатов редакции плана

**2026-10-13 · CP6 — четыре файла поручений опубликованы; README согласован с параллельными дополнениями; ключевые стыки проверены повторным чтением.** Это редакция плана, не выполнение R01–R25, не обучение и не независимая Astra Ultra экспертиза.

## Опубликованные артефакты

| Этап | Артефакт | Commit |
|---|---|---|
| CP1 | Первичные выводы и границы; историческая версия этого журнала | `5ea0d07d29d7ce76c508de403d4f584507b9c113` |
| CP2 | [R01–R10: источники, решения и реальные алгоритмы](LEARNING-WORK-ORDERS-01-10.md) | `bcf077a14c24a217471f215309454bad25c29935` |
| CP3 | [R11–R15: архитектура, DPO/kNN/LoRA, оценка и экспертиза](LEARNING-WORK-ORDERS-11-15.md) | `af89f074aa3eabc65ec639cc6eb0f69980a12fd7` |
| CP4 | [R16–R20: требования и implementation packets](LEARNING-WORK-ORDERS-16-20.md) | `12a6ffae6163a43044d59a87e8ccc24ba8c516d2` |
| CP5 | [R21–R25: curriculum, causal experiments, PC/feedback и передача](LEARNING-WORK-ORDERS-21-25.md) | `15f6becac857f57b70beff005ccd0a39bd3bdb3c` |
| Промежуточное сохранение по повторному запросу | Журнал CP5 со ссылками на четыре файла; после публикации работа продолжена | `2037e778382d8c710dbcc99faba9a6077a7bf3c6` |
| CP6 | [README: согласованный вход и промежуточные публикации](README.md) | `10415b3bcb58cf6bff0c8b95ea5fd06e326395b5` |

Прерывание ответа перед подтверждением CP5 не отменило запись: commit `15f6bec...` установлен через историю файла и remote readback. CP — checkpoints редакции, не дополнительные research rounds или product experiments. Полный прежний CP5-журнал сохранён в commit `2037e778...`.

При первой попытке финального обновления этого журнала GitHub вернул **409: branch HEAD изменился во время записи**. Повторное чтение подтвердило неизменный blob журнала `62daa12477da38b5686b42011060d347456af2fb`; повторяется только собственная запись поверх свежей ветки. Это конфликт конкуренции, не отсутствие write-доступа. Force-push не используется.

## Что подтверждено повторным чтением

Зафиксированный candidate проверки — `10415b3bcb58cf6bff0c8b95ea5fd06e326395b5`, не обещание неизменности движущейся ветки.

| Файл | Content blob на candidate | Проверка |
|---|---|---|
| LEARNING-WORK-ORDERS-01-10.md | `da5f428aeea6ec91f6d71acf7c140ce22305fff7` | Строки 35–61: независимые inputs R02/R03, WorkLeft/RAG и сохранение требований. Общая часть имела readback после CP2. |
| LEARNING-WORK-ORDERS-11-15.md | `ddb3faa4cac3b8a772394951c82746eb8383e6dd` | Blob подтверждён на candidate; предметно прочитаны R12 objective/attribution и R13 71–96: operational versus bulk, same-panel и первый recipe. |
| LEARNING-WORK-ORDERS-16-20.md | `066ee7b8fb5f3704feea3197696a07cf25140129` | Строки 63–103: R18 А/Б/В, примеры/края/приёмка; R19 extension; R20 actual prompts. |
| LEARNING-WORK-ORDERS-21-25.md | `f6262531298b74c6f70a5d5ec66579fd00cd254b` | Начало при продолжении; на candidate конец R25 с next packet, extension и границей независимости. |
| README.md | `f0a2bc33534ba779e79a503a9e60b9887bc558f3` | После записи прочитано начало: журнал, четыре файла, независимые R01–R03 без фиктивного DONE. |

Это **адресная документная сверка**, не полный автоматический обход всех ссылок/архивов, не выполнение prompts и не independent review.

## Параллельная работа сохранена

R01–R03 продолжают свои snapshots/results; этим автором их файлы не переписывались, статусы не повышались. Подтверждённый тематический subset можно передавать без выдуманного общего DONE. Новая редакция не требует restart.

При продолжении обнаружены и прочитаны параллельные дополнения: PLAN-25-RUNS blob `b0346b68232d61a5785ba40c927b16eaf6eca392` (начало/основная часть; длинный конец возвращён частично), START-NEXT-RUN blob `788e9accab662f4453b5d0114fa02d7aff10dace` (1–42), RECOVERED-DESIGN-CONTRACT blob `907ebb04ad9f560c88dd3760a963356d59c5c81d` (A01/A02), RECOVERED-DESIGN-PROMPTS blob `f4a982fa9b0b171bb63396d8f7bf4982e8fdd580` (1–26). Их полное содержание не объявлено проверенным здесь.

Эти файлы **не переписаны поверх старого дерева и не присвоены текущему автору**. README согласован с их маршрутом: один полный Rxx + его HYBRID + уникальная RECOVERED-дельта и нужные H/A. Сохранены Graph как machine specialization, StackDetector-before-normalization, domain/intent/stack, TensorCore/shared base/project adapters, BM25+kNN, positive/negative/warning memory/final arbiter, TeachingVector/Context/UnitId до actual trainer. Исторические проценты/размеры/20× compression не считаются текущими measurements.

## Что именно исправлено

В старом primary R01–R10 ещё были обязательные inputs R01 для R02 и R01/R02 для R03, а общая часть направляла writers в shared results/SOURCES.csv. CP2 исправил **сами prompts**, а не только override в README: независимые прямые источники, собственные results/Rxx, retained work и адресные handoffs.

R04–R07 требуют проследить representation→typed consumer, pair→objective→mutation→ordinary inference, domain pool→metric/k→learned authority→arbiter; исследовать correction/higher-order/async связи. R12 выбирает конкретный baseline/challenger, математику и owner, а не заново перечисляет блоки. Потеря rich context в простом DTO не объявляется фактом без проверки сериализации/loader/consumer.

R13 разделяет operational one-pair решение и bulk-level research claim: статистическая значимость после каждой одиночной пары не стала новым автоматическим gate. Возможная проблема накопления слабых улучшений оформляется как policy-гипотеза R12/R16, не тайное разрешение replay/batch promotion.

R18 получает первый recipe от R13 и сначала готовит smallest missing code seam, не ждёт всего curriculum. R21 содержит семь domain passports с meaningful negatives. R22 задаёт различающие causal contrasts, не полный factorial. R23 учитывает полный footprint и feedback stateflow. R25 требует конкретное следующее действие, а не гарантированный выигрыш алгоритма.

README больше не утверждает, что R13 уже выполняется: это назначенное будущее поручение. Первый валидный опыт может закончиться отсутствием улучшения. Промежуточные публикации содержат реальные источники/результат/ограничения/остаток, не пустые carriers.

## Основания и честная глубина чтения

Исходный HEAD CP1 — `dd89e61d8c3f480644b5187663c5660154252510`. По началу и позднему продолжению previous rounds thinking results.md сохранены полезное разделение механизма/обучения/поставки и граница прежнего S04-only обзора. Исторические 33 задания, sandbox-ссылки и proposed sessions не current evidence; вся промежуточная история не прочитана.

Прочитанные R01 F01–F09 дали WorkLeft/RAG roots, S00–S10 и aliases; R02 §§1–3 — typed roles, deterministic generation versus learned authority, Builder и оценщика; R03 F01–F08 — комбинации одного input, память/арбитр, replay conflict и разные таксономии. Это handoffs с ограничениями, не присвоенный полный аудит или measured learning.

Напрямую прочитаны core sections: trainable-architecture-decision-SKILL 1–105; phase-planning-SKILL 105–165; plan-review-SKILL 205–270; generated-code-review-SKILL 1–80; prompt-patch-authoring-SKILL 1–100; output-readability-gate-SKILL 1–90. Доступность experiment-design-SKILL и improvement-measurement-SKILL дополнительно подтверждена началом 1–12, не полной загрузкой. Все пути — docs/skills в core; full library load не заявляется.

Прочитаны `00.Core/DynamicGraphContracts.cs` 1–125, `00.Core/LearningMachineContracts.cs` 1–95, `10.LoRA/LoraLinear.cs` 1–145, WorkLeft RESEARCH-01 1–35. Это contracts/projection/policy, не полная trace всех consumers и не efficacy.

Проверены первичные методические ссылки R12: [DPO §4/eq.7](https://arxiv.org/html/2305.18290), [LoRA abstract](https://arxiv.org/abs/2106.09685), [learned Mahalanobis/LMNN abstract](https://jmlr.org/papers/v10/weinberger09a.html). Они поддерживают описание методов, не доказывают current implementation или преимущество XIIGEN. Читались HTML, не PDF.

## Сохранённые решения и следующий путь

Четыре роли сохраняются по смыслу: Unit — domain/vector/output schema и typed many→many; Flow — отношения/коррекция; Orchestrator — async combinations; DynamicGraph — higher-order patterns/Builder. Реальные API и training owner устанавливаются по коду; duplicate interfaces по aliases не создаются. У каждого алгоритма/связи есть input/output meaning, deterministic часть, learned state/authority, objective/signal, owner, alternatives и falsifier.

DPO validity отдельно от Student utility; TIE/BOTH_BAD/unknown не forced preference. Standard DPO требует подходящей policy/reference, similarity не log-probability. Graph→unit attribution/payload/loader прослеживаются. Bulk — наблюдения новых разрешённых уроков; нет скрытого replay или второго gradient поверх неизвестного состояния. Retry после полного restore определяется первичной действующей policy.

Vectors/kNN/LoRA/paths оцениваются независимыми задачами, mapping/admissibility/quality/time/retention и полной ценой. LoRA не tokenizer; coordinate/index consumer обязателен; similarity не отменяет arbiter. Missing contract/authority/data admission нельзя скрыть под READY; empirical uncertainty получает опыт. R13 first recipe и R18 first seam не ждут всей поставки.

Baseline 25 сохранён. По разрешению пользователя объективно необходимый уникальный остаток может получить полную extension-карточку PLAN §4.1: сначала reuse/перераспределение, затем prompt/context/skills/refs/examples/edges/examination, workload и явное число вызовов. Автоматических R26/30/50 нет; эта редакция дополнительные исследовательские раунды не выполняла.

Следующий вход — [START-NEXT-RUN](START-NEXT-RUN.md). Уже работающие R01–R03 продолжают свои остатки, не начинают заново. Полный обход библиотеки, остальные Rxx, реализация code gaps и реальные learning experiments остаются работой назначенных проходов, не результатом публикации текста.

## Граница результата

**Learning gain, компактность, PC-fit — NOT_MEASURED. Независимая Astra Ultra examination — NOT_PERFORMED.** Product code/tests/workflows/weights/datasets этим автором не менялись; build/trainer/Windows/Actions вручную не запускались. Параллельные результаты не отменялись. GitHub publication не означает запись на локальный C:.
