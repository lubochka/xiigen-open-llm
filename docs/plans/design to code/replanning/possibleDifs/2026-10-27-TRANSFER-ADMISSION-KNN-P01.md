# Продолжение подготовки переносов: KNN, дозы обучения и достоверность результата

27 октября 2026. Фаза — READ_ONLY_RESEARCH и запрошенные Markdown. Продуктовый код, tests, runners, данные и веса в этой работе не изменялись; обучение и builds/tests не запускались. Срез исходников/результатов — `75107af930a1d4c55d60fa8a4ef7c01643f13cb0` в `C:/Projects/llm_mvp_core`.

Действующие планы находятся в canonical repo, а этот документ сохраняет след исследования в библиотеке:

- [69: критерии допуска и доказательства](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/69-transfer-admission-and-priority-2026-10-27.md) — приоритет выигрыша более5%, затем меньшего; CPU-исключение не распространяется на память или accuracy. Неизмеренные ресурсы не объявлены безопасными.
- [74: два недостающих переноса KNN](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/74-knn-domain-constructor-transfer.md) — дополнительный constructor с массивом функций, проверенные профильные rankers, learned routing к разрешённым trained state+principles, native writer, restore и ProductHost E2E. FIX-KDP и E2E-KDR зарегистрированы как CONDITIONAL.
- [68/U8: ложная подпись DPO4](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/68-transfer-repair-followup-plan-2026-10-27.md#p01-dpo-receipt) — P01 выставляет `dpo_ran=true` до первого успешного StepVector и сохраняет DPO4 после отказа, хотя updates=0. Это ошибка диагностики, не доказанный дефект trainer. План общего step-helper проверяет порядок успешных callbacks и emitted receipts.
- [63§8: граница опыта144](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/63-paired-learning-effect-and-factor-contrasts.md#dose144-boundary) — Sign14/21→20/21 полезен; Magnitude gold rank ухудшен на29.62%. Обе руки имеют290updates, поэтому сокращение общего обучения не доказано.
- [Обновлённый реестр](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md) — прежние60 stable ID сохранены, добавлены2 новых; указаны зависимости и остаток допуска.

Новые подтверждённые пробелы исследовательского инструмента:

1. KDR qualification сравнивает TopProfile с expected, но не CanSelect. У RU/HE метка правильная, однако рабочий router отклоняет запрос по margin. Обученность метки не доказывает доступный путь к модели.
2. KDP restore читает examples.jsonl, затем подменяет Text/Output/Instruction/Prefix исходным fixture по ExampleId. Сохранённые40/40 не доказывают восстановление payload из файла; повреждение ответа может быть скрыто подстановкой. Новый regression должен читать только сохранённый bundle, а gold оставлять вне loader.
3. P01 aggregate2/4 скрывает смену состава: rank8 TRAIN1/2→2/2, но EXAM1/2→0/2. Общий rank8-default не допущен. Отдельно обнаружено противоречие stage/counts выше.
4. Native KNN Unit имеет Train(corpus), а experimental KDR writer использует Index.Add. Поэтому обещание одного incremental presentation при переносе было неверным. План74 назначает13 native Train и105 предъявлений, включая9строк для S2; равенство исходному Add ещё предстоит проверить.

Один Astra ultra worker `/root/repair_numerical_audit` подготовил74 и вернул отдельные замечания к69/63/68/03. Родитель сверил основные counts и source oracle. Это чтение существующих свидетельств и проверка документов, не независимое повторение опытов. Первоначальные результаты и старые карточки сохранены. Новые гипотезы универсального scheduler, global rank8 и полноценной RU/HE-маршрутизации результатами не объявляются.
