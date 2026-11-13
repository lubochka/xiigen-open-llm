# E-OPT-STEP: исправление описания сохранённого опыта без изменения raw

**Обновление core6e3:** [measurement-erratum](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/measurement-erratum.md) уже документирует неверные labels, все GC и границы атрибуции. Исправление описания учитывается как выполненное другой документной сессией. Этот addendum сохраняет независимое исходное чтение core827; raw не меняется. LF-03 теперь также завершён; текущий NEXT — N63-INGRESS-LENGTH, см. [поздняя сверка](11-late-reconciliation-6e3-eaed.md).

**Core `827a1acdbef95d220886e54f9d36bce0b405c8cd`. Новых запусков, правок старого raw или product changes нет.** Опыт №06 завершён по actual A/B receipts: 2 issued / 2 completed на плечо. Он остаётся кандидатом. В снимке 827 следующим был LF-03; нынешний порядок указан в обновлении выше. Ранний файл со статусом PREPARED описывал момент до появления этих receipts; он не назначает повтор.

## 1. Ошибка описания установлена, другой executed factor не доказан

В raw `experiment=E-OPT-STEP-01`, но `factor` и `measured_path` буквально соответствуют R1. В actual source общего bridge найдены те же константы. Отдельный A/B source diff, напротив, реализует metadata reader для шага optimizer. [Независимый адресный source review](reviews/EOPT-independent-source-review.md) содержит все primary permalinks и состав изменённых файлов.

| Поле / артефакт | Что сохранено | Аддитивное уточнение |
|---|---|---|
| `result.factor` A | original element-wise LINQ bit comparison of frozen base | Правильный A-фактор: `ExportOptimizerState().Step` для RNG callback при сохранённых m/v. |
| `result.factor` B | bitwise byte-span comparison of frozen base | Правильный B-фактор в source: `ReadOptimizerStepForRandomState` → `AdamW.ReadPortableMetadata` + validation; payload m/v не копируется ради номера step. |
| `result.measured_path` | `DpoProjectLoraRecipient.RequireValid` | Это stale R1 descriptor. В обоих actual A/B frozen-base comparison остаётся LINQ; B не содержит R1 span fix. |
| `result.core_commit` | Старый `62bcc8f...` literal | Рассматривать как унаследованное поле исходного bridge. Реальные source files закреплены readback на 827; поле само не доказывает исторический binary hash. |
| `Prepare-Opt.ps1` | Копирует bridge из N62-R1 | Источник описательной ошибки находится здесь и в bridge literals277–281. |
| Variant props / A/B files | Раздельные `DpoTrainer.cs`, `AdamW.cs`, `AdamWPortableState.cs` | Source/build-схема соответствует одному EOPT фактору. Устойчивость всех runtime guards и точная полная historical binary цепочка не восстановлены этим чтением. |

**Raw остаётся байт-в-байт прежним.** Для следующего иного назначенного опыта нужно передавать descriptor его fixed study в экспериментальный bridge и проверять совпадение ID/factor/path перед первым учебным вызовом. Это исправление оснастки, не новый самостоятельный эксперимент и не повод переисполнить EOPT. Product code не следует менять из-за неверной строки отчёта.

## 2. Полезный результат и регрессии

| Показатель | A | B | B−A |
|---|---:|---:|---:|
| Allocated bytes полного учебного цикла | 3 764 994 488 | 3 764 526 544 | **−467 944; −0,01242881%** |
| TRAIN elapsed, мс | 50 068,549 | 40 665,43 | −9 403,119 |
| CPU, мс | 37 687,5 | 35 828,125 | −1 859,375 |
| GC0 / GC1 / GC2 | 193 /19 /13 | 189 /14 /10 | **−4 /−5 /−3** |
| Process peak, байт | 840 916 992 | 1 045 463 040 | **+204 546 048; хуже** |

Исходные значения и ссылки на A/B сохранены в [recomputed JSON](evidence/saved-results-recomputed.json). GC1/2 присутствуют в raw, хотя их польза не была показана в коротком report. Полезная измеренная дельта времени/CPU сохраняется как успех соответствующей оси; причинное ускорение не установлено одной fixed A→B парой. Неизменённые backward/optimizer/forward фазы также существенно отличаются по времени.

Расчёт 425 984 bytes m/v payload и измеренные 467 944 bytes — разные свидетельства. Остаток не разложен экспериментально по объектам. Поэтому «одна такая пара показала...» корректнее универсального «детерминированно всегда экономит...».

## 3. Граница реального измерения

Bridge имеет непустой RNG callback, поэтому изменённое выражение достижимо. После первого реального update moments уже созданы; второй update способен выявить цену полного копирования. Это реальный project-LoRA learner, не detached DTO benchmark.

Окно включает два `StepVector`, внешние exports для budget/receipts, сериализацию receipts и checkpoint publication. Обе руки используют одинаковый scope. Поэтому это стоимость **данного полного учебного цикла с сохранением**, не latency одного getter и не чистый optimizer kernel.

Recorded final weights/optimizer/reference равны по сохранённым hashes/receipts; внутренние bytes всех checkpoint payloads этим аудитом не десериализовались (часть Git-объектов — LFS pointers). 16 adapter arrays изменились, 39 base arrays не изменились. До/после TRAIN NLL/rank/exact в bridge отсутствуют. Online losses двух разных уроков не заменяют такую панель. Нельзя выдумать её из final hash, объявить output quality улучшенной или назначить повтор для дополнения данных.

Вывод для решения пользователя: **сохранён полезный ресурсный кандидат с совпавшими записанными состояниями на малом наборе, видимой peak-регрессией и отдельным дефектом описания происхождения**. Полная эквивалентность всех failure paths и причинная устойчивость времени не доказаны. Кандидат переноса — [C-06](CANDIDATES.md).
