# Независимая проверка карточки E14 и общего контракта

**Дата:** 2026-10-19. **Рецензент:** /root/dpo_lora_audit. **Scope:** дизайн E14 N-KU-01 и общий learning-experiment contract; source review, без запуска. Это не независимая проверка собственных ND-DPO-SFT/ND-DPO-PAD.

## 1. Точная версия рассмотренного текста

| Документ | Прочитанный scope | SHA-256 файла |
|---|---|---|
| audit/agents/knn_units.md | Разделы 1–2: новый producer дефект и N-KU-01; весь текст просмотрен, исторические receipts здесь заново не проверялись | ddf626a77ac338b63ef352878a4c3c200a877b63a7add404fcc5573a4185f737 |
| audit/deliverables/59-TRAINING-PATH-AND-EXPERIMENT-VALIDITY-2026-10-19/05-LEARNING-EXPERIMENT-CONTRACT.md | Полный документ, 61 строка | 833fb38e9444482e7fcc50f5c25e6dfba9be06ecee240aaaaacc148ccac88666 |

Эта проверка относится к указанному тексту. Смысловые изменения после этих hashes требуют адресного readback изменённых мест, а не автоматического наследования review. Оригинальная protected папка не менялась.

**Вердикт:** DESIGN_REVIEW_NO_BLOCKING_CAUSAL_ISSUE_WITH_CLARIFICATIONS. Три требования пользователя выполнены в дизайне: малый реальный training опыт; любое полезное измеренное улучшение признаётся успехом по показателю/домену; вмешательство воздействует на обучение, а не на посторонний замороженный тест. Данные/host ещё предстоит подготовить; runnable/native-PASS и измеренного выигрыша этот review не заявляет.

## 2. Независимый code trace E14

Источники закреплены на core 8e020052fae7efa694b8d33f90768648d6b67af9.

| Проверяемая связь | Прямое доказательство | Оценка |
|---|---|---|
| Нынешняя учебная метка учитывает лишь ForEach/For/While | [FileActionKnn.cs, 41–55](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/12.SemanticCompiler/Prediction/FileActionKnn.cs#L41-L55) | Два предложенных дополнительных AST kinds действительно меняют producer train-label |
| Ранний null-priority способен закрыть нужную ветку | [Тот же source, 32–38](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/12.SemanticCompiler/Prediction/FileActionKnn.cs#L32-L38) | Карточка удерживает member-access/null и сложные fallback условия вне опыта |
| Сигнал входит в настоящее обучение | [E14ContextActionKnn.cs, 34–69](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/35.ExperimentsV2/Experiments/E14ContextActionKnn.cs#L34-L69) | Features+Label передаются в native Train; historical driver использует тот же producer и для validation, что объясняет необходимость независимого gold |
| Обновляется обучаемая память | [FileActionKnn.cs, 121–126](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/12.SemanticCompiler/Prediction/FileActionKnn.cs#L121-L126) | Train очищает/заменяет весь corpus. Полные наборы 4→6→8 в карточке соответствуют этому API |
| Предсказание зависит от обученной памяти | [FileActionKnn.cs, 128–141](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/12.SemanticCompiler/Prediction/FileActionKnn.cs#L128-L141) | Cosine top-k и majority vote. EXAM не подменён прямым вызовом исправленного Label |
| Feature profile остаётся постоянным | [FileActionKnn.cs, 75–107](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/12.SemanticCompiler/Prediction/FileActionKnn.cs#L75-L107) | Нет одновременного добавления новых признаков; их недостаточность может ограничить эффект |
| Есть условный продуктовый consumer | [SemanticCompilerFacade.cs, 193–202](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/12.SemanticCompiler/SemanticCompilerFacade.cs#L193-L202), [327–333](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/12.SemanticCompiler/SemanticCompilerFacade.cs#L327-L333) | ≥4 examples, feature flag и C# обязательны. Карточка правильно не утверждает включённый caller конкретного RP01 run |

Успешное снижение ложного ADD_LOOP — содержательный узкий показатель. ADD_METHOD описан как fallback текущей учебной таксономии, а не как универсально лучшая правка программы. Recall недостающих циклов, exact label accuracy и прежние полезные случаи остаются рядом, поэтому возможный ущерб от чрезмерного выбора fallback не скрывается.

L0 — общий уже обученный baseline; его создание не выдается за measured gain без before-панели. Наблюдаемые L0→L1→L2 изменения происходят при реальных последующих вызовах Train. Нулевое число neural optimizer steps для этого memory learner корректно и не делает опыт zero-learning.

## 3. Общий контракт

Поддержаны следующие пункты:

- Вопрос назван через приобретение, сохранение или стоимость обучения; owner/state/consumer обязательны.
- Простое добавление несвязанного Train не легализует frozen serving, report-field или file-only опыт.
- A/B содержит одинаковый старт, один фактор, независимую frozen panel, фактические state changes и единицы измерения.
- Малыми измерениями не создаётся автоматическое требование accuracy gain, +5%, p-value или общего promotion. Неопределённость и шум всё равно сообщаются.
- Сырые outputs, внутренние дельты каждого плеча, B−A и регрессии перечислены.
- CODEX astra 6 ultracode и внешняя открытая китайская модель получают разные заданные роли; фактические вызовы не выдуманы.
- HOST_MISSING не переименован в существующий CLI. Production admission не ослабляется.
- Историческое изменение margin, нулевая retention и копирование archival receipt не превращены в новые measurements.

Контракт пригоден как общий дизайн этих пяти предложений; он не заменяет конкретные материалы, source-family split и подключение native host каждой карточки.

## 4. Две точности для итогового пакета

**C1 — граница ND-SFT в общей таблице.** В строке E-DPO-SFT-ORDER общего контракта выражение «incremental-путь» лучше заменить на «C# exact-v2 research profile; найденный mixed incremental caller использует legacy». Полный DPO-отчёт теперь явно разделяет эти пути и требует отсутствующий research harness. Это уточнение scope, а не независимый approval собственных ND-карточек данным рецензентом. Root уведомлён.

**C2 — число предъявлений и cost E14.** 8 — число уникальных TRAIN source; за три Train наборы 4+6+8 дают **18 предъявлений на плечо, 36 всего**, 6 Train calls и 48 Predict. Producer Label/FeatureVector preparation нужно измерять отдельно от Train kernel (Clear/AddRange). Если сообщается полный cost acquisition, включать изменённую label-preparation; kernel-only время не называть стоимостью всего обучения. В тексте уже правильно сказано о повторе полного corpus; это уточнение бухгалтерии и границ метрики, не изменение причинного дизайна. Root и автор E14 уведомлены.

## 5. Skill и claim scope

Использованы ранее прочитанный learning-path-audit и релевантные части generated-code-review; для этого review непосредственно прочитаны разделы plan-review о candidate binding, trainable material quality, plan-only границе, deterministic evidence, audit sufficiency и domain scorer. Полное чтение многотысячного plan-review файла не заявляется. Правила навыка применяются в пределах текущего заказа на аудит и документы; старые требования исполнять production/repeated-pass цепи не активированы.

Что мог бы дополнительно найти более широкий аудит: фактический запуск UseFileActionKnn в продукте, устойчивость feature collisions на большем корпусе, полный DPO materializer из E14, действительные native outputs и статистику затрат. Эти вопросы не исследованы этим review и не скрыты под общим PASS.

Новые эксперименты, build/tests, toy measurements, вызовы моделей, git mutations и публикация не выполнялись. Исправление собственного dpo_lora.md после замечаний другого агента — авторский fix, не дополнительный независимый review.

## 6. Последняя отдельная проверка root-карточки E-SFT-RESPONSE-MASK

Прочитан полный файл 06-SFT-RESPONSE-MASK-EXPERIMENT.md, SHA-256 3bc2471853837aa3bb3582ecdeccedcb1613b9679fe3848815d63be7cdd6280c. Автор карточки — root, поэтому эта проверка независима от её автора. Непосредственно сопоставлены Trainer.cs 102–147,376–426, TensorOps.cs1600–1624, IncrementalTrainer.cs402–418 и tokenizer Encode/Decode interface. Новых broad searches и исполнения не было.

Блокирующего причинного дефекта не найдено: два actual updates обязательны, full joint IDs одинаковы, masked loss и backward согласованы, denominator не меняется, source-family split и правило успеха присутствуют. Отсутствие готового masked Trainer/spans API честно указано.

Три точности перед реализацией:

1. Определить T как tokenIds.Count. Настоящий Trainer подаёт в Model.Forward tokens[0..T−2], targets — tokens[1..T−1] (102–117); выражение «полная последовательность» в строке Model input не должно означать дополнительный токен во Forward. Для row i mask применяется к byte-span target tokenIds[i+1], не input tokenIds[i]. В обоих плечах сохранить одинаковый sample weight и знаменатель T−1: loss=weight/(T−1)·Σ mask[i]·CE[i], logits-gradient=weight/(T−1)·mask[i]·(softmax−oneHot). Prompt+LF входит в служебный prefix; boundary policy задана до freeze.
2. Fixed LR конкретизировать как одинаковый положительный MaxLr=MinLr и WarmupSteps=0 либо прямо использовать уже описанную checkpoint-backed perCallConfig. У Trainer реальный GetLr и проверка LR>0 находятся на128–132; нельзя оставить defaults WarmupSteps100 и считать один лишь термин fixed LR готовой конфигурацией двух updates.
3. Фиксированный denominator сохраняет коэффициент response contribution **до clipping**. Удаление prompt terms закономерно меняет общую norm и может изменить срабатывание ClipGradNorm (126), затем AdamW trajectory. Это часть эффекта маски, не скрытая вторая переменная, но нельзя обещать равную norm или одинаковый effective response update. При интерпретации фиксировать clip factor/norm внутри тех же actual steps, не добавляя самостоятельный benchmark.

