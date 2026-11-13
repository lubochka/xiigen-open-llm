# Последнее согласование: LF-03 завершён, соседние гипотезы сохранены

**Действующая очередь: 40 уникальных ID, 7 завершённых, 33 будущих; NEXT №08 `N63-INGRESS-LENGTH`.** Это документная сверка primary результатов и параллельных изменений. Новых запусков, builds, product fixes и изменений защищённой папки не было.

## 1. Какие источники обновились

| Источник | Закреплённый срез | Проверенная дельта |
|---|---|---|
| core | `6e3e8864933e35fccdb4f21369c9912fa195aaf6` после `827a1acdbef95d220886e54f9d36bce0b405c8cd` | LF-03 A/B, study/report/provenance/source; EOPT erratum; 27 изменённых карточек, 0 новых/удалённых |
| planning | `eaed843ad2b063fbb003f073deb69da19f0cb135` после `53cbcabd3926cda714bab428d294eb666301e9e4` |62 добавленных файла, из них 33 Markdown получены и сверены по Git blob SHA; две новые tensor-карточки |

Все 45 production project trees core6e совпадают с 827; ранее проверенные 9 проектов совпадали между 669 и 827. Поэтому source pin669 в новых VT/RMS не устарел по коду. Изменены экспериментальные/docs roots, а не соответствующая production реализация. [Tree-квитанция](evidence/core6e3-production-tree-delta.json). Последний список 157 карточек — [inventory](evidence/core6e-cards-inventory.json).

## 2. LF-03 нельзя назначать повторно

Первичные [A](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/A/arm-result.json) и [B](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/B/arm-result.json) завершены; каждый issued1/completed1. A закончено 17: 25: 22 UTC, B17: 25: 53 UTC. Это сильнее устаревшего NEXT07 в [index](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md). По следующей строке index действует №08 N63-INGRESS-LENGTH. LC-REF-CACHE расположен №14, он не следующий.

| Измерение | A | B | Вывод |
|---|---:|---:|---|
| Allocated bytes в фактической учебной фазе |1 848 491 176|1 847 844 360|−646 816; успех по стоимости выделений |
| Wall этой фазы, мс |15 270, 2952|17 178, 4596|+12, 50%; ухудшение в этом запуске |
| Process CPU, мс |14 406, 25|15 015, 625|+4, 23%; ухудшение |
| Process peak, байт |830 795 776|787 300 352|−43 495 424; наблюдение всего процесса |
| GC0/1/2 |93/8/6|90/5/3|−3/−3/−3 |
| TRAIN first-token NLL before→after |6, 8145495665→6, 8142643885|6, 8145495665→6, 8142643885|Учебный сдвиг одинаков; benefit B по NLL=0 |
| Gold rank / exact |221 / false|221 / false|Изменения нет; full-answer accuracy не измерена |

Все 16 LoRA arrays меняются, 39 base arrays frozen; reference неизменен, optimizer step1 и fresh model/optimizer/RNG readback совпадают между arms. Бюджет двух arms: 2 updates, 4 policy forwards, 4 reference forwards, 4 backward; дополнительно 4 диагностических forwards. Teacher/generate/warmup/repeat=0. [Полный пересчёт](evidence/core6e-delta.json), [первичные файлы и hashes](evidence/core6e-source-manifest.json).

**Ограничение происхождения не скрыто:** study ссылается на правильный hash manifest, но B Tensor hash в manifest — placeholder; bridge hash не совпадает с current bridge bytes. Нынешние A/B copies содержат ожидаемую разницу, но historical B binary binding не засвидетельствован полностью. Payload расчёт 608 872 не равен measured646 816; остаток 37 944 не атрибутируется одной строке без дополнительных данных. Результат остаётся полезным кандидатом; повтор ради provenance или красивого отчёта не назначается. [Кандидат с точным original method](02-measured-candidates.md#7-lf-03-завершённый-настоящий-dpo-полезная-экономия-и-более-медленная-фаза).

EOPT [measurement-erratum](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/measurement-erratum.md) уже признал старые factor/path labels и manifest gaps; старые report/study/raw bytes сохранены. Нельзя представить признанный document-gap как всё ещё скрытый. Erratum NEXT LF-03 устарел; inherited neural future IDs теперь 12.

## 3. Соседние гипотезы включены с сохранением происхождения

Обе карточки, их встроенные prompts, общий execution contract, README/entry и registry прочитаны на planning eaed. Они принадлежат [соседнему опубликованному пакету](https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/README.md). Они не выдаются за открытия этого аудита.

| ID | Единственный factor | Назначенный host / entry |
|---|---|---|
| [GRAD-ACCUM-COPY-01](https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/GRAD-ACCUM-COPY-01.md) | Первый `TensorOps.AccumulateGrad`: убрать внешний `gradToAdd.Data.ToArray()`, копию TensorStorage оставить |`C:/xg/AGC1/host`; `pwsh -NoProfile -File 'C:/xg/AGC1/host/Run.ps1'` |
| [FROZEN-BROADCAST-MAP-01](https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/FROZEN-BROADCAST-MAP-01.md) | `MultiplyBroadcast`: не создавать `mappedBIndices` при двух `RequiresGrad=false`; координатный алгоритм прежний |`C:/xg/FBM1/host`; `pwsh -NoProfile -File 'C:/xg/FBM1/host/Run.ps1'` |
| [VT-APOSTROPHE-01](cards/VT-APOSTROPHE-01.md) | NativeTrainOnExample теряет часть required term из-за parser апострофа |Полный fixed recipe, 2Train/arm; новый проект этого аудита |
| [RMS-ROW-REDUCE-01](cards/RMS-ROW-REDUCE-01.md) | Координатные копии `SumAlongAxis` внутри настоящего SFT/LoRA |Полный fixed recipe, 4updates/arm; новый проект этого аудита |

Все четыре — пока **не измеренные гипотезы**. RMS не совпадает с frozen broadcast: разные методы/allocations. VT не совпадает с target scopeFileActionKnn: другой учебный получатель и parser. LF-03 относится к начальному градиенту `Backward(initialGradient)`; GRAD касается первого добавления чужого градиента `TensorOps.AccumulateGrad(target, gradToAdd)` — сопоставлять надо эти exact sites, а не общую фразу «двойная копия». [Проверка сохранённых A/B и manifest](evidence/lf03-vs-grad-accum-dedup.md).

Объединение множеств: 38 внешних ID +38 прежних ID этого пакета −36 общих =40. Хвост сохраняет опубликованный порядок: **BPE34→GRAD35→FROZEN36→VT37→RMS38→IDENTITY39→KN40→END**. Только внешний NEXT FROZEN→IDENTITY заменён на FROZEN→VT; прежние карточки и результаты не переписываются. [Receipt сравнения](evidence/concurrent-eaed-learning-merge.json).

## 4. Остаточные ограничения постановок

29 будущих inherited output roots и entry совпали с 827 authoritative headers. Исправления body paths на 6e не создают новое назначение или новый attempt. LF-03 исключён из future output map. Карта добавляет только AGC1/FBM1 и собственные VT/RMS. [Точные выходы](05-output-contract.md).

P54 остаётся **0/7 полных runtime recipes**: все семь NEEDS_INPUT_FOR_RECIPE сохранены. Разрешение читать уже обученную память есть; отсутствие checkpoint/fixture/host не превращает эти проверки в не относящиеся к обучению. На 6e изменена только одна строка NEXT; P52/P106 уточнения уже внесены раньше. [Адресная проверка](evidence/current-p54-delta.md), [таблица недостающих входов](03-experiment-coverage.md).

Новизна собственных VT/RMS проверена на 832 planning Markdown: 799 исходных +33 добавленных. Это полный download/hash/search корпус и адресный смысловой разбор совпадений, а не обещание построчного аудита всех 832 документов и всех веток. [Поздний readback](evidence/concurrent-planning-readback.json). Старые review receipts относятся к их названным срезам; текущие counts/NEXT находятся в [04](04-execution-registry.md) и [JSON](execution-registry.json).

**Немедленный STOP/NEXT сохраняется:** после команды перейти не продолжать прошлые report/hash/review/Git/retry. При полном STOP прекратить работу. Старые опыты и частичные результаты не удалять; никакого повторного обучения ради дооформления. Любое измеренное полезное улучшение — успех своей оси и темы, с ухудшениями рядом; перенос в production решает пользователь отдельно.
