# Окончательный срез827: шестой результат, LF-03 next и несогласованность E-OPT raw

**Источник статуса зафиксирован на core ¤827a1acdbef95d220886e54f9d36bce0b405c8cd¤. Следующий невыполненный — №07 LF-03.** [Первичный index](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md) подтверждает завершённые01–06 и36 известных ему ID. Добавив только GRAD/FROZEN, получаем [единую программу38 ID:6 completed,32 pending](04-execution-registry.md). Более поздние branch HEAD в эту аттестацию не включаются; документирование не разрешает runtime.

## 1. Новая несогласованность в шестом опыте

E-OPT-STEP имеет два actual COMPLETED arm JSON, issued2/completed2 на плечо. Study/report называют фактором чтение optimizer Step через metadata без копирования m/v. Но оба arm JSON содержат `measured_path=11.DPO/DpoTrainer.cs::DpoProjectLoraRecipient.RequireValid`, а A/B `factor` описывают исходное LINQ bit comparison и byte-span comparison frozen base — то есть фактор R1.

**Это конкретное противоречие attribution, не доказательство отсутствия обучения и не достаточное доказательство исполнения именно R1 вместо E-OPT.** Метки могут быть устаревшими; имеющихся raw недостаточно для однозначной связи измерения с заявленным B-factor. Raw нельзя переписать под отчёт; production-перенос не следует из одного названия experimentId. [Exact readback, источники и candidate](experiments/FINAL827-EOPT-READBACK.md).

| Измеренная метрика | A→B | Наблюдение |
|---|---|---|
| Allocated bytes |3 764 994 488→3 764 526 544 | **−467 944**, полезный measured gain |
| GC0/1/2 |193/19/13→189/14/10 | **−4/−5/−3**, полезные наблюдения всех поколений |
| Train elapsed, мс |50 068,549→40 665,430 | −9 403,119; causal speedup не установлен |
| Train CPU, мс |37 687,500→35 828,125 | −1 859,375; причинная доля конкретной правки не изолирована |
| Whole-process peak, байт |840 916 992→1 045 463 040 | **+204 546 048**, ухудшение |

Любой полезный measured знак сохраняется как успех своей оси/домена, рядом с регрессиями. Это не сертификат конкретного patch. State/readback receipts не заменяют after-panel: TRAINv2 и post-update target quality отсутствуют. Старому запуску новые поля не приписываются. [Числовой JSON](experiments/FINAL827-EOPT-METRICS.json) фиксирует исходные поля и расчёты. Existing E-OPT candidate уже есть в protected; второй ID, rerun или production fix не создаётся.

## 2. Защита от повторного запуска по устаревшему шаблону

На827 index закрывает E-OPT, но N62-карточка всё ещё содержит future E-OPT текст и SelectedId. [LF-03 overlay](experiments/FINAL827-EOPT-READBACK.md) задаёт единственное разрешённое будущее назначение: LF-03 SourceRoot/HostRoot/results, SelectedId=LF-03, соответствующий guard и один Run-With-Learning-Signal.ps1. Budget2 native updates+4 DIAG_TRAIN forwards; NEXT N63-INGRESS-LENGTH. Inline пример — назначение будущей материализации, не существующий запущенный script. Protected results/required-changes не являются outputs этой работы.

Завершённые01–06 не повторять ради новой панели, исправления меток, проверок binary custody или текущего отчёта. Прямой NEXT/STOP немедленно прекращает оформление прежней задачи; новое состояние реестра не создаёт исключения.

## 3. Исправленные назначения outputs

[Root05](05-executor-contract.md) и [машинная проекция](coverage/execution-projection.json) содержат32 активных output assignments. Из32 inherited planning53cb assignments26 активны без изменения,2 архивированы completed,4 заменены либо исправлены; добавлены2 наших tensor assignments. Прежние значения сохранены для provenance, не как альтернативные команды.

| ID | Окончательное назначение и причина |
|---|---|
| NG01 | Internal NG01/r/attempt-0001, report.md/candidate.md. На827 добавились owner/queue paragraphs; источник, данные и пути669 сохранены |
| AST-SIB-CACHE-01 | Internal AST01/results/attempt-0001, candidate-fix.md; единственный Run-Assigned.ps1 вместо прежних Planning outputs |
| BPE-REALIZED-MERGE-COUNT-01 | Internal BR01/r/0001, report.md/candidate.md; единственный Run-Assigned.ps1 |
| ROPE-TRAIN-TABLE-01 | RPE01/a001/results, **candidate.md**. Source-назначение не менялось: candidate-fix.md был ошибкой прежнего mapping, которую здесь исправили |

[Подробные literal paths, source pins и доказательства](history/07-core827-output-delta.md), [machine delta](history/evidence/core827-changed-output-assignments.json). Новый root не разрешает обход старого claim/partial или повтор ранее issued попытки. AST/SKNN остаются одним фактором; новых дублирующих карточек нет.

## 4. Что остаётся новым и не измеренным

GRAD-ACCUM-COPY-01 и FROZEN-BROADCAST-MAP-01 не становятся выполненными из-за новых R2/E-OPT результатов. На827 TensorOps/TensorStorage имеют те же blobs, что883/669: `e1a20dca84d76ee85d0c559bd496c1fa3c96ac8b` и `22aec3ba03b756530001ae786acbdce28af90104`. Source-copy883 остаётся назначенным. Их budget, Student, данные, single-factor diff, A0/A1/B0/B1, model/skills, русский report и абсолютный NEXT/STOP закреплены в карточках.

P54 blanket конфликт и protected NG01 presence уже исправлены на669; конкретные P54 inputs и точная phase-only проверка остаются ограничениями в прочитанных рецептах. Полный новый semantic audit всех protected документов827 не заявлен. Historical E07 conditional остаётся вне очереди до настоящего cumulative caller.

## 5. Завершённая граница работ

Этот пакет публикуется только добавлением в planning possibleDifs. Core/protected, production/tests/CI и старые результаты этой работой не менялись. Все new findings и readbacks привязаны к pinned sources; более поздние изменения репозиториев потребуют отдельного назначения, а не бесконечного продолжения старого анализа.
