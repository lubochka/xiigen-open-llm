# Адресная проверка плана ремонта переносов

27 октября 2026, Asia/Jerusalem. Независимый reviewer: **`/root/transfer_math_audit`**. Это **вторая отдельная документная задача**: review [64](../64-transfer-execution-audit-2026-10-27.md), [65](../65-transfer-repair-plan-2026-10-27.md) и новых первых абзацев [41-grad](../41-grad-accum-copy.md)/[42](../42-frozen-broadcast-map.md). Она не является вторым runtime-прогоном, повторной квалификацией семнадцати переносов или полным аудитом репозитория.

Фаза **READ_ONLY_RESEARCH + Markdown**. Source HEAD проверен: `4ce8bdd0dd3942a20b32c569c2b0f0c37894dd23`. Читались эти документы и только адресные зависимости: планы45/53/57, E10 Run и его исторический parent, AT01 factor из первого пакета, AGC/FBM factor/ownership. Build/tests/discovery/inference/training/mutation-run не выполнялись. Изменены только этот review и, по поручению родителя, уточнены owner links в разделе8 первого [math-пакета](2026-10-27-math-transfers-review.md).

В первой прочитанной версии65 были два существенных противоречия: единая формула `B=transfer commit, A=B^` не квалифицировала будущий ремонт ATS и игнорировала common seam до SVD baseline; обязательный дополнительный настоящий SVD Run не был включён в сохранённый бюджет. В первом абзаце41 был перепутан владелец. Замечания переданы родителю, который правит планы. Ниже сохранён исходный смысл замечаний; это история review, а не утверждение, что исправленные формулировки остаются неверными.

## P1. Historical transfer A/B нельзя применять как универсальное доказательство ремонта

**Исходная версия65:110:** для каждого FIX назначала `B=его transfer commit`, `A=B^`, общий harness. Для ATS это означает B=`046e0ded876`, то есть ту generic-оптимизацию, которую R2:61–71 требует заменить direct rank-4 branch. Этот pair годится исследовать первоначальный transfer, но не может подтвердить исправленный factor: repaired B в нём отсутствует.

Для SVD `git show 3852e774fa9^:34.Experiments/Experiments/E10SvdPmiEmbedding.cs` показывает private `TruncatedSvd` и `ArgmaxDot`; `FitFixedTraining`/`EvaluateFixedPanel` отсутствуют. [57:130](../57-native-transfer-contract-gaps.md) и [исходный владелец53:37](../../fixes-based-on-experimentsperiments/53-hierarchical-history-and-directed-svd.md) назначают одинаковый mechanical extraction **до A**. Один harness на raw parent/new transfer сам по себе это не обеспечивает; без совместимого адаптера baseline не может вызвать новый native seam. Нельзя обходить проблему копией learner в Tests.

**Ремонт плана:** три отдельно названные задачи и receipts: historical-transfer pair; repaired-factor pair; current-composition correctness. Для repaired ATS — B с narrow direct path, A с тем же общим source и историческим coordinate-array fallback, без direct branch; все соседние факторы одинаковы. Для отсутствующих seams — общий проверенный mechanical overlay с собственными diff/SHA, без преждевременного переключения A на B-algorithm. Сохранять base/seam/factor/harness identities отдельно. Не выбирать неизвестные будущие SHA фиктивно сейчас.

**Промежуточный readback исправления:** в65 появились отдельные **Historical-transfer pair**, **Repaired-factor pair**, **Current-composition correctness**; прямо сказано, что A с overlay не является нетронутым B^ и старый ATS commit не проверяет direct-path repair. Эта смысловая поправка закрывает исходное противоречие. Перед будущим запуском всё равно нужны конкретные разрешённые директории и реальные SHA; review их наличия не утверждает.

## P2. Настоящий E10 Run требует отдельного счёта, помимо fixed-panel capture

**Исходная версия65:79–83:** требовала проверять настоящий Run и одновременно сохраняла ровно4 SVD fit +2 Bigram fit /9000 token presentations /30 predictions. Из source [E10:69–140](https://github.com/lubochka/llm_mvp_core/blob/4ce8bdd0dd3942a20b32c569c2b0f0c37894dd23/34.Experiments/Experiments/E10SvdPmiEmbedding.cs#L69-L140) видно, что отдельный успешный Run сам выполняет Bigram.Train, counts→PPMI→SVD и два validation прохода. Простое добавление Run к основному capture не помещается в прежний бюджет. Мой первый M4-пакет также требовал consumer assertion, не назначив его отдельный расход; это ограничение предложения исправлено здесь явно.

**Выбранный родителем bounded companion:** ровно один Run на **repaired B**, вход500 IDs `i % 3`, `i=0…499`. Существующий split даёт400 train/100 validation. Поэтому дополнение равно **1 SVD +1 Bigram fit**, **800 token presentations**, **99+99=198 predictions**. Основная программа остаётся4+2/9000/30; полный новый budget — **5 SVD +3 Bigram fit,9800 token presentations,228 predictions**. Это арифметический вывод из source, не выполненный запуск.

Независимый gold — следующий token цикла0→1→2→0. Первые400 tokens дают399 соседних пар, ровно133 каждого directed ребра; PPMI на этих трёх рёбрах равна `log2(3)`, обратные рёбра имеют0. Ожидание **99/99 относится к Bigram baseline и directed SVD enhanced**, не к symmetric source-source reader. Product VocabSize500/rank30/seed42 менять не нужно; три активных ID допустимы текущим Run. Это companion подключения, не новая acquisition- или speed-qualification.

**Readback исправления:**65 явно добавил companion и новый budget;57 должен содержать тот же addendum. Чтобы бюджет не расползся снова, аналитический контрпример «заменить только Run reader на symmetric» нельзя назвать выполненной mutation-проверкой. Реальный второй mutated Run добавил бы ещё2 fits/800 presentations/198 predictions и нуждался бы в отдельном назначении. Данный бюджет включает только один нормальный companion.

## P2. Номер41 обозначал два разных документа и размывал владельца

**Исходный первый абзац41-grad:3** говорил «для перечисленных здесь переносов CS/MB», затем обсуждал RPE и generic ATS. Но title, решение и product target этого файла — **FIX-AGC / первая защитная копия AccumulateGrad** (41-grad:1,7,38–60). CS/MB/RPE назначены в другом существующем файле [legacy41-neural-learning-resource-deltas.md](../../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md), что прямо подтверждает57:113. Поэтому ссылка «/41» без имени файла могла отправить исполнителя к чужому factor/бюджету.

**Ремонт:** first paragraph41-grad должен защищать именно AGC: первая копия остаётся у Tensor/TensorStorage, последующие вклады суммируются прежним `+=`, мутация исходного `gradToAdd` после первого накопления не меняет target. Не менять конструктор на shared storage и не переносить соседний `Tensor.Backward` factor. Для CS/MB/RPE в65 и общих таблицах дать полное имя/ссылку legacy41-neural. Для FBM использовать canonical42. В своём первом math-пакете reviewer уже заменил неоднозначные owner references этим различением; исходный legacy-документ не менялся.

## Что не является находкой

- R2 coordinate formula `source=((b*S+s)*H+h)*D+d`, `destination=((b*H+h)*S+s)*D+d` соответствует AT01. Нужные rank4/normalized1↔2 guard, прежний fallback и inverse-gradient явно сохранены; численного возражения к этому ремонту не найдено.
- Новый первый абзац42-FBM соответствует [первичному report:11–17](../../../../../experiments-v2/RP01-MICRO/FBM1/results/report.md): оба RequiresGrad=false, численное произведение прежнее, mixed/trainable branch не должна терять gradient. Кэш API там не назначен. Рост wall/CPU/GC указан отдельно от allocation gain и совпадает с таблицей42:20–27.
- 64 не называет source review выполненными tests и честно ограничивает independent coverage семью переносами. В этой отдельной проверке не проводился второй полный аудит остальных десяти переносов; они не получают дополнительного independent PASS из факта чтения64.
- Повторная проверка локальных Markdown targets в64/65/41-grad/42 вернула0 отсутствующих путей. В частности, актуальные ссылки TensorOps уже содержат `00.TensorCore/Core/TensorOps.cs`. Это link inventory, не доказательство семантической правильности всего плана.

## Итоговая граница

**Финальный readback выполнен:**65:114–116 разделяет historical/repaired/current pairs и provenance общего seam;65:85–87 назначает companion только на repaired B, точные5+3/9800/228 и исключает дополнительный mutation-run.57:3 содержит тот же бюджет и точную формулировку «gold99/99 для Bigram baseline и directed SVD enhanced».41-grad:3 теперь относится к AGC, проверяет независимую защитную копию и накопление `[1,2]+[3,4]=[4,6]`; CS/MB/RPE ведут на явный legacy41-neural. Исправления этих трёх замечаний **подтверждены содержанием**, не только сообщением родителя. Первоначальные формулировки сохранены выше как история review.

В оговорённом узком объёме открытых существенных замечаний после этого readback не осталось. Это документный verdict по указанным условиям плана, не общий PLAN_READY всего проекта и не подтверждение будущих runtime/state/resource результатов. Локальные ссылки обоих worker-пакетов проверены:0 отсутствующих путей. Продуктовых `.cs` изменений не обнаружено последним `git diff --name-only -- '*.cs'`; никаких запусков продукта не было. Будущее исполнение требует отдельного прямого поручения.

`claim_scope`: один адресный независимый review документации; `evidence_scope`: source/Git и непосредственно прочитанные версии64/65/41-grad/42 и адресных dependencies; `count_claimed`:1 review packet этой задачи, не tests; `independent_evidence_count`: этот самостоятельный пакет; `subagent_id`:`/root/transfer_math_audit`; `downgrade_needed`: все runtime/state/learning PASS остаются неподтверждёнными. Plan-review skill прочитан адресно: раздел566–596 и headings; полной загрузки4273 строк не заявлено.
