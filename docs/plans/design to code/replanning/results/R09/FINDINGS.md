# R09 — реальная цена процесса и настоящие blockers

**Текущий статус после RCOR-2026-10-13:** результаты R09 согласованы интегратором по прямому поручению Лубы. Статистика исходных 30 runs — **ATTRIBUTED_AGGREGATES / RAW_SAMPLE_NOT_RECONSTRUCTED**; количественный bottleneck и экономия из неё **НЕ ПОДТВЕРЖДЕНЫ**. Это не повтор R09 и не завершение его эмпирического остатка. Исходные IDs и полезные выводы сохранены; новая запись отдельно от первоначального автора в RUN-LOG. Product learning/PC fit — NOT_MEASURED.

Новые Actions/Windows runs, обучение, тесты, код, datasets и weights ни исходным R09, ни этой интеграционной поправкой не запускались и не изменялись.

## 1. Короткий вывод

**Исправленный вывод:** различие между инженерной целостностью, качеством обучения и native-поставкой остаётся обоснованным предметом плана. Но утверждение, что именно стоимость Windows уже количественно доказана как главный текущий bottleneck, было сильнее сохранённого evidence: исходные 30 строк и формула длительностей не восстановлены (§2). Нельзя выводить фактическую экономию или критический путь работы только из этих агрегатов.

R05 локализовал DPO/tokenization/objective seam. Это позволяет рассматривать ранний обучающий опыт отдельно от нерелевантной native qualification, **но ненативная математика не доказывает переносимость существующего bootstrap или отсутствие native-зависимости выбранного consumer**. Удалять gate с маршрута можно по его причинной связи с конкретным изменением, не по неподтверждённой стоимости.

Тезис «все тесты подгоняются под систему» по рассмотренному evidence не доказан. Первоначальный R09 сохранил наблюдение конкретной native-проверки PsecPhysical (§3); интегратор не выполнял её заново. Blanket-delete проверок не следует ни из гипотезы подгонки, ни из нынешнего ограничения статистики. Разделение сохраняется:

1. **детерминированные contract/integrity checks** — data/state/provenance/no-repeat/rollback;
2. **learning examination** — before/after quality, domain mapping, DPO utility, dynamic kNN/tokenization, retention, cost;
3. **native/host delivery qualification** — Windows process/security/resources/UI/extension, только там, где capability действительно пересекает эту границу.

Это согласуется с сохранённым WorkLeft-контрактом R09-S01: проверки следуют за работающей возможностью, а не образуют очередь ради каждой строки плана. Его прежнее чтение атрибутировано исходному R09; не новый runtime observation.

## 2. Исходная выборка: сохранённые утверждения, а не проверенные измерения

**RCOR-R09-01 — исправлено непосредственно у источника утверждения.** Прежняя редакция этого раздела представляла агрегаты как наблюдаемый runner occupation и достаточно сильное доказательство bottleneck. Текущий вердикт: **RAW_SAMPLE_NOT_RECONSTRUCTED; численные выводы не приняты как проверенные**. Предыдущие bytes сохранены в Git: `a93f725088f12e7f994fddc9c6d206aa879b9342`, этот же путь, blob `1498851b4378722ae6ee8e728124aad846b22131`.

**Заявленное исходным R09 правило отбора:** первые 30 branch-runs, возвращённые GitHub Actions API для codex/dpo-implementation на момент R09; заявленное окно 2026-10-12T05:38:45Z — 2026-10-13T12:23:11Z. Интегратор не восстановил этот ответ API и не заменял его текущими первыми 30 runs.

| Семейство | Заявленные runs | Заявленные outcomes | Заявленная сумма, min | Заявленная median, min | Текущий статус всех чисел |
|---|---:|---|---:|---:|---|
| S03 Windows Verification | 13 | 5 success, 7 failure, 1 in-progress | 368.3 | 38.1 | ATTRIBUTED / RAW_SAMPLE_NOT_RECONSTRUCTED |
| Design-to-Code Focused Verification | 17 | 16 success, 1 cancelled | 114.2 | 6.9 | ATTRIBUTED / RAW_SAMPLE_NOT_RECONSTRUCTED |

**Прежние отношения 3.2× и 5.5× не использовать как подтверждённую экономию, стоимость или сравнительную эффективность.** Правильное арифметическое отношение неподтверждённых агрегатов не подтверждает их происхождение. Неизвестность не означает, что числа доказанно ложны; означает, что проверяемой основы пока недостаточно.

Для принятия исходной выборки нужны её raw records: run_id, attempt, head_sha, workflow, status/conclusion, start/end, состав и интервалы jobs, handling cancelled/in-progress, а также точная формула и знаменатель. Run elapsed, сумма job elapsed, queue wait, billed usage и время человека различаются; нельзя заранее назвать сумму runner occupation, не определив, что именно суммировалось. Overlapping jobs не складываются как wall-clock человека. Новая выборка с новым cutoff допустима как отдельное исследование, но не как восстановление старой.

Отдельные три job rows в `COMPLETION-REPAIR-INTEGRATION-2026-10-13.md` §5, включая сообщённые 20 минут 53 секунды одного Windows-job, — атрибутированный отдельный эпизод, **не восстановление 30 runs**. Интегратор не выдаёт чтение пересказа за повторное чтение raw jobs.

Основание согласования: `R17-EVIDENCE-POLICY-2026-10-13.md` §1 на `a6bede9aced047b416a4973745a3e2618c7163f5`, blob `2b99b93c55381e8acdf946a575640fa8cb022600`; этот раздел прочитан непосредственно. Исправлены также R09-S07, D-R09-01/03/08, R09-C01/C04 и отдельная запись журнала, чтобы qualification не оставалась только во внешней policy.

## 3. Representative episode: RED Windows не равен learning failure

**Сохранённое наблюдение первоначального R09, не повторённое интегратором:** existing run 38379535495, job 115236293190 завершился RED. В исходном validation evidence указаны focused wrapper exit code 1, TRX outcome Failed и identity `LlmFromScratch.Tests.CodeDocumentationPreparedBaselineTests.PsecPhysical`.

Это ограниченное evidence native boundary, а не статистическое доказательство всей кампании и не доказательство систематической подгонки. PsecPhysical может защищать реальное свойство процесса/ресурса.

Этот episode **не измеряет** качество DPO chosen/rejected, сохранение различающего условия после tokenization, response-conditional objective, изменение Student quality, domain mapping, dynamic kNN metric/k/radius, ordinary inference benefit или bulk-to-bulk accuracy/time.

Следовательно, native guard сохраняется для затрагивающей его capability. Его перенос с critical path первого lesson допустим только после проверки, что выбранный learner/ordinary consumer не зависит от защищаемого им свойства. **Локальная Windows-зависимость, GitHub Windows qualification и remote S05 — разные рёбра**, а не одно общее требование.

## 4. Гипотеза подгонки тестов

### Verdict по рассмотренному evidence: **INCONCLUSIVE глобально**

Нельзя вывести «система подгоняет тесты» только из RED→GREEN. Нужна цепочка requirement → implementation → failed assertion → exact assertion/rubric diff → reason → behavior after diff и классификация LEGITIMATE_FIXTURE_REPAIR / AUTHORIZED_REQUIREMENT_CHANGE / WEAKENED_EXPECTATION_WITHOUT_AUTHORITY / ORACLE_FITTING / UNRESOLVED.

В рассмотренном первоначальным R09 a5de1a8 → dcdded6 episode есть production/test changes и большой массив generated review/evidence artifacts. Этого чтения недостаточно, чтобы назвать конкретное изменение assertion «подгонкой». Episode остаётся UNRESOLVED, не обвинительным выводом.

**Уточнение RCOR:** объём preimage/readback/review/evidence material поддерживает гипотезу process-overhead risk, но не измеряет длительность труда, стоимость и размер ускорения. Оптимизация по causal relevance возможна без доказательства test-fitting и без выдачи §2 за подтверждённый benchmark. R05 seam не требует нового trainer; необходимость конкретного host guard проверяется отдельно.

## 5. Что R05 меняет в причинной картине

R05 как атрибутированный code trace обнаружил два предметных разрыва:

1. LogicalDpoTrainingUnit.BuildBatch нормализует whitespace и обрезает prompt/chosen/rejected до 480 символов перед tokenization; значимое различие exact pair может исчезнуть до optimizer.
2. DpoTrainer.ComputeLogProbs считает mean по prompt+answer, тогда как действующий design требует response-conditional objective.

R05 не доказал ordinary consumer опубликованного checkpoint. Это граница того trace, не доказательство отсутствия generator во всём repo и не отмена поздних source-backed результатов R18.

Исправленная последовательность раннего опыта:

**exact permitted pair → lossless/domain-sensitive representation → correct response objective → one bounded update → isolated candidate examination → accept/retain/recover → cold ordinary consumer выбранного состояния**.

Same-panel parent/candidate comparison предшествует promotion. Windows включается по фактической зависимости выбранного host/consumer/implementation seam, не по ненативности формулы и не по старому порядку task rows.

## 6. KEEP / REUSE / DEFER / REPLACE-AS-EVIDENCE

| Класс | Решение | Почему |
|---|---|---|
| Provenance, permissions, immutable identities, no-repeat, rollback/recovery | **KEEP** | Защищают корректность механизма, но сами не доказывают learning quality. |
| Native Windows process/security/resource behavior | **KEEP at affected native milestone** | Сохранено ограниченное исходное evidence PsecPhysical; применимость устанавливается для конкретного пути. |
| Focused verification / build-once batching | **REUSE where relevant** | Предусмотрено WorkLeft для затронутых deterministic seams; количественный выигрыш R09 не подтверждён. |
| Repeated full/native Windows qualification before unrelated learner steps | **DEFER only after causal classification** | Отсутствующее dependency edge нельзя выдумать; существующее нельзя удалить. Retained owner — host/native delivery milestone. |
| Green-test count / coverage as proof of learning | **REPLACE-AS-EVIDENCE** | Learning quality — controlled behavioral examination, не число green tests. Existing technical checks не удаляются. |
| Full domain matrix before first valid lesson | **DEFER as global barrier** | Первый experience использует sufficient lawful family; остальные domain obligations сохраняются и расширяются после working loop. |
| Новый trainer/framework до устранения R05 seam | **DO NOT ADD by default** | Existing trainer/update path уже code-traced; сначала exact input/objective/consumer seam. |

## 7. Каким должно быть learning examination вместо coverage proxy

Для каждого selected learner/domain сохранить bulk_id; subject/domain/intent/project; lesson_count; exact pair id; semantic recipient; representation/tokenizer version; kNN metric/k/radius if used; parent/candidate checkpoint; same-memory no-update control; before/after quality; uncertainty; mapping accuracy; path quality; retention; latency; total compute; peak RAM/VRAM; persistent bytes; ordinary-consumer outcome.

Перед опытом определить metric/unit, useful-effect threshold, retention tolerance, independent-family estimator/sample/looks и finite budget. Same-panel comparison внутри bulk; stable anchor и отдельная сложность новых families между bulk. Перефразировки одного source не independent N. Неизмеренное не заполнять нулём или claim improvement.

### Роли DPO — не смешивать

- **CODEX, requested Astra 6 UltraCode / project alias GPT-6-Astra ultra** выполняет человеческий педагогический выбор разрешённого урока.
- **Внешняя открытая китайская модель** создаёт обе окончательные стороны chosen/rejected.
- **CODEX той же запрошенной конфигурации** отдельно оценивает exact pair в exact permitted context.
- Student обучается только на допущенной паре; слабый Student output — возможный context, не автоматически final rejected.
- DPO reference, pair judge, runtime arbiter и independent Student evaluator — разные роли.

Никакая часть R09 или RCOR не выполняла эти product calls. Запрошенный alias не actual invocation evidence.

## 8. Hybrid deterministic + learned: что проверяется функционально

Сохраняются Unit — domain/input-vector/output schema, typed many→many associations; Flow — relations/correction; Orchestrator — async combinations и стоимость всех branches; DynamicGraph — higher-order relations/compositions.

Для следующего experiment явно фиксировать и менять нужный слой: frozen Units/changed Flow; fixed deterministic candidate generator/learned final authority; exact neighbor/ANN; fixed representation/domain-aware ranking; fixed ranking/changed k; отдельно multi-neighbor use; no-update/one-update при сопоставимой памяти; primitive/higher-order composition; current/domain-sensitive lossless tokenization.

Это causal experiment design, не выполненные вмешательства. Большее k при first-hit Output не fusion; новая LoRA matrix не сама по себе semantic token projection; выигрыш solver не обучение selector. Значимость комбинации deterministic+learning измеряется downstream utility/cost, не фактом исполнения pipeline.

## 9. PC feasibility — что R09 НЕ измерил

Исходные непроверенные CI aggregates ничего не доказывают о полном PC training footprint. Пока **NOT_MEASURED**: base Student/runtime, encoder/tokenizer, active LoRA, reference, optimizer/gradients/activations, kNN index+embeddings, graph/cache, snapshots, reindex, rejected attempts и teacher/judge external cost.

R23 владеет whole-path footprint/time-to-quality measurement. Adapter size, CI duration и сумма несовпадающих component peaks не заменяют simultaneous observed peak и полезность на пользовательском hardware.

## 10. Downstream handoff после RCOR

- **R11/R14:** causal DAG и capability queue, не прежние количественные ratios. Каждый deferred Windows gate сохраняет native owner/trigger; фактический host dependency может остаться необходимым.
- **R13:** isolated same-panel parent/candidate + same-memory no-update, predeclared domain/family/retention/budget; promotion только после оценки. Полный hybrid scope не сводится к E1.
- **R17:** тройное разделение mechanism/learning/native, а статус исходных aggregate claims теперь совпадает с policy §1 непосредственно в R09.
- **R23:** R09-S07 не использовать как verified process-cost metric. Получить исходную raw sample/formula либо отдельную новую атрибутированную выборку, а PC learning cost измерять независимо.

## 11. Falsifiers и незакрытый остаток

Пересмотреть affected decision при доказанном native dependency первого learner/consumer; при wider guard, защищающем обязательное состояние эксперимента; при exact assertion-authority trace, позволяющем определить test-fitting; либо при восстановленной/новой сопоставимой cost sample.

**Текущий остаток:** исходные raw 30 records/formula не восстановлены; global test-fitting INCONCLUSIVE; actual learning/PC gains NOT_MEASURED. RCOR закрывает противоречивую публикацию этих claims и их передачу, **не эмпирическое исследование R09**. Исторический журнал и IDs сохранены, актуальный вывод находится здесь и в DECISIONS/SOURCES/CHANGE-MAP.
