# Поздняя дельта core: восстановленное обучение и новые конфликты карточек

**Свежий core: `81141d57b847bc050881c2fabba8302ac18c0f8a`.** Commit «8th experiment middle results and plan conflicts resolution» появился после исходного pin `dd150aa5` и проверен перед публикацией. [Compare](https://github.com/lubochka/llm_mvp_core/compare/dd150aa5216e42b8a8bba14955337398ec5f8397...81141d57b847bc050881c2fabba8302ac18c0f8a) содержит один commit, семь добавленных файлов: пять protected Markdown, JSON source closure и отчёт RESTORE-MINIMAL-SET. Изменений исходного Trainer/LoRA/optimizer в этой дельте нет.

Получены новые документы, исходник/проект/launcher восстановительной пробы, raw JSON и Git LFS pointers двух сохранённых артефактов. Никакого нового обучения, загрузки модели в runtime или повтора пробы не выполнялось.

## 1. Положительный результат действительно есть

Проба использует исследовательский Student **H16, V256, один блок, q/v LoRA rank2/alpha4**, две короткие C# строки One/Two. Выполненная в другой сессии траектория:1000 SFT updates → SaveCheckpoint/ExportPortableState → новый объект → LoadCheckpoint/TryImportPortableState → ещё200 updates. Это реальный native `Trainer.TrainSupervisedNextTokenStep`, а не внешний численный surrogate. [Program:36–128](https://github.com/lubochka/llm_mvp_core/blob/81141d57b847bc050881c2fabba8302ac18c0f8a/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs#L36-L128).

Из [raw](https://github.com/lubochka/llm_mvp_core/blob/81141d57b847bc050881c2fabba8302ac18c0f8a/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/restore-minimal-set.json) пересчитаны именно сохранённые показатели:

| Метрика | Восстановленное состояние после1000 | После ещё200 | Вывод |
|---|---:|---:|---|
| TRAIN response NLL, среднее по двум строкам | 4,766696420187372 | 4,711598363502413 | **Успех NLL: −0,055098056684959** |
| TRAIN средний gold rank | 51,825 | 50,050 | **Успех rank: −1,775** |
| Правильные response tokens TRAIN | 3/40 | 4/40 | **Успех token accuracy: 7,5%→10%**, +1 token |
| TRAIN строк, в которых есть хотя бы один правильный response token | 2/2 | 2/2 | Это наличие совпавшего токена в строке, не правильные ответы |
| Полностью правильные TRAIN responses даже с teacher forcing | 0/2 | 0/2 | Правило пользователя о ненулевых полных ответах не выполнено |
| Правильные EXAM response tokens | 0/43 | 0/43 | Общий навык не продемонстрирован |
| Полностью правильные EXAM responses с teacher forcing | 0/2 | 0/2 | Не выдавать за ненулевое решение задачи |
| CONTROL Pos response NLL | 5,807963371711153 | 5,857194149707198 | **Регрессия: +0,049230777996046** |
| CONTROL Pos gold rank | 95,85714285714286 | 100,5 | Регрессия примерно+4,643 |

TRAIN aggregate3/40→4/40 получен сложением `ResponseTop1Correct` и `ResponseTargetCount` двух строк, не новым inference. Все поля всей сохранённой панели `afterPriorTraining` и `afterRestore` совпадают при прямом сравнении JSON-объектов.

**Этот результат должен остаться успешным кандидатом по NLL, rank и token accuracy данного домена.** Он не становится неудачным из-за отсутствия роста полной точности. Но он также не закрывает отдельно поставленное пользователем условие «0 правильных ответов до и после не приемлемо». Обе оценки совместимы; одну нельзя подменять другой.

## 2. Почему «2 из2» нельзя читать как два правильных ответа

В [Program:207–225](https://github.com/lubochka/llm_mvp_core/blob/81141d57b847bc050881c2fabba8302ac18c0f8a/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs#L207-L225) счётчик `TrainRowsWithAnyCorrectResponseToken` увеличивается, если `ResponseTop1Correct > 0`. Отдельный `TrainRowsWithExactResponse` увеличивается только при правильности всей response. Raw явно содержит2 для первого и0 для второго на всех трёх стадиях.

Кроме того, [Score:228–261](https://github.com/lubochka/llm_mvp_core/blob/81141d57b847bc050881c2fabba8302ac18c0f8a/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs#L228-L261) подаёт полный правильный префикс последовательности (`teacher forcing`) и оценивает shifted targets. Свободная autoregressive генерация не выполняется. Сам [отчёт](https://github.com/lubochka/llm_mvp_core/blob/81141d57b847bc050881c2fabba8302ac18c0f8a/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/report.md) честно говорит, что полностью точного ответа не было, но одновременно объявляет опыт валидным по правилу пользователя на основании двух строк с отдельными правильными токенами. Это **адресный конфликт интерпретации**, исправляемый из существующего raw без запуска и расхода учебных токенов.

Корректная формулировка для будущего отчёта: «Восстановленная исследовательская модель продолжила улучшать NLL, rank и token accuracy на двух учебных строках; полный teacher-forced exact остаётся0/2, свободный ответ не измерен. Ресурсная стоимость подготовки1000+200 updates учитывается отдельно. Диагностичность полного task answer для следующих A/B пока не установлена».

## 3. Что действительно доказано о восстановлении

| Утверждение | Доказательство и предел |
|---|---|
| Поименованные model tensors восстановлены точно | `StateHash` хеширует key, длину и float bits. Raw даёт одинаковый hash до сохранения и после Load, а fresh object до Load отличается. Это сильное свидетельство для данного H16 state. |
| Импорт optimizer состоялся и global step сохранён | Raw `optimizerImported=true`, step1000→1000; после продолжения1200. Отдельного сравнения всех m/v bytes до и после import в raw нет. Не называть это выполненным побайтным сравнением optimizer snapshot. |
| Панель восстановленного состояния воспроизводится | Прямое read-only сравнение **всех сохранённых JSON-полей** before/after совпало. В самом коде boolean `panelIdenticalAfterRestore` проверяет лишь средний TRAIN NLL и count-any-token; этот слабый предикат не является общим полным comparator. |
| Первый loss продолжает кривую | Недостаточно для вывода о moments. Последний prior update обучает Two, первый continued — One ([чередование:83–85,126–128](https://github.com/lubochka/llm_mvp_core/blob/81141d57b847bc050881c2fabba8302ac18c0f8a/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs#L83-L128)); это разные targets. |
| Восстановлен весь production lifecycle | Не проверено: здесь другой профиль, private snapshots, dropout0, нет испытания production promotion/admission и полного stochastic resume. |

Имена сохранённых артефактов реальны. В Git лежат LFS pointers:

- `results/trained-weights.bin`: oid SHA256 `ba8ec8c20250bc431da4bf9993ff22c508422cd7224b5c57cec33ce7b98c6e0e`, actual size43500.
- `results/trained-optimizer.bin`: oid SHA256 `603cf13423f7c6ae5af073a4ea824f49009175b145ce551f6c1b5cf59b66842a`, actual size1112.

[Weights pointer](https://github.com/lubochka/llm_mvp_core/blob/81141d57b847bc050881c2fabba8302ac18c0f8a/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/trained-weights.bin), [optimizer pointer](https://github.com/lubochka/llm_mvp_core/blob/81141d57b847bc050881c2fabba8302ac18c0f8a/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/trained-optimizer.bin). Бинарные LFS payload здесь не скачаны и не восстановлены: наличие pointer+raw не равно проверке доступности payload на будущем execution host. H16 state не совместим по topology с H512 B0 четырёх карточек и с H64 SEXP01 без изменения отдельного рецепта.

## 4. Кандидат в исправления, который сохраняет удачный результат

**RESTORE-MINIMAL-SET / REPORT-SEMANTICS-01 — документный кандидат, не новый эксперимент.** Исправить вывод валидности и названия метрик в отчёте, сохранив все raw и полезные gains:3/40→4/40 tokens, NLL−0,055098, rank−1,775; рядом0/2 complete response и ухудшение CONTROL. Не приписывать проверку m/v bit equality, product promotion и достаточность1000/200. Пример будущего связанного исправления reader — `Program.cs:119–121,183–189`: если этот инструмент когда-либо будет отдельно изменяться, полную панель сравнивать целиком, а `learningPresentBeforeContinuation` не использовать как full-response exact. Это адрес для решения пользователя, **не поручение менять код или повторять probe сейчас**.

Фраза «не работает принятие» пока сильнее данных. Проба сама сообщает, что ничего не передавала в каноническое дерево. Непринятый private candidate может отражать исследовательский scope, который пользователь как раз требует сохранять до решения о переносе. Нужен отдельный точный admission/promotion caller и результат отказа, чтобы объявить этот компонент сломанным. Также из одного профиля/бюджета нельзя вывести универсальное «любой опыт1–4 updates обязательно даст0→0».

## 5. Две новые карточки: историческое расхождение и позднее согласование

После дельты в protected корпусе **367 Markdown и163 карточки**: прежние362/161 плюс5/2. Основной `00-execution-index.md` в этом commit не изменён и по-прежнему содержит40 позиций.

| Новая карточка/промпт | Адресный конфликт | Решение в этом дополнении |
|---|---|---|
| [ATTN-TRANSPOSE-STRIDES-01:4](https://github.com/lubochka/llm_mvp_core/blob/81141d57b847bc050881c2fabba8302ac18c0f8a/docs/plans/replanning-possible-diffs/experimets/cards/ATTN-TRANSPOSE-STRIDES-01.md#L4) | Назначает себе№39 и NEXT JSON№40, но действующий index39=IDENTITY-M1-01,40=KN-ROUTER-LEARNED-KEY-01. Рецепт снова4updates на cold H64; диагностичность full answer не доказана. | Сохранить как прямой learning cost candidate. Поздний planning согласовал ATTN39/JSON40 и перенёс старые consumers на41/42; protected index не переписывается. До диагностичного рецепта не считать ready. |
| [JSON-OPTIONS-REUSE-01 executor:17](https://github.com/lubochka/llm_mvp_core/blob/81141d57b847bc050881c2fabba8302ac18c0f8a/docs/plans/replanning-possible-diffs/experimets/prompts/JSON-OPTIONS-REUSE-01-executor.md#L17) | Назначает себя№40, NEXT IDENTITY№41; этого маршрута нет в текущем index. | В согласованном planning —№40, **прямой memory-learning candidate**: TrainStructuredBatchAsync → ExecutorUnit.TrainOnExample → deserialize → mutation memory. Приоритет обучения; не понижать до посторонней сериализации или позднего readback. |
| Оба новых cards/prompts, заключительный пункт | Требуют писать в protected `required-changes.md`, что конфликтует с текущим прямым запретом пользователя. | В этой сессии писать только planning possibleDifs; успешный кандидат/report добавляются здесь. Историческая инструкция другой сессии scope не расширяет. |

При будущем **одиночном** поручении конкретного ID использовать `NEXT=END`. Для общей очереди поздний [planning registry](https://github.com/lubochka/xiigen-planning/blob/2d1741261e9e17c0e4dec94a480b5711f5ad47d3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-dd150aa/REGISTRY.md) уже согласовал42 ID; **следующий№09**, потому N63I№08 имеет raw на исходномdd150aa. Историческое расхождение core index не является нерешённым выбором исполнителя. JSON options внутри Executor.TrainOnExample относится к первому приоритету обучения. Сохранён порядок09–38→ATTN39→JSON40→A1/A2; нашA3 — отдельная авторская подготовка перед зависимыми consumers41/42. Подробнее [добавочный реестр](07-additive-registry.md) и [первичный №08](12-current-queue-and-n63-evidence.md).

Ни ATTN, ни JSON не дублируют SFT-EXP-REUSE-01: первый меняет strides в Transpose, второй повторное использование JsonSerializerOptions, новый кандидат — повторные MathF.Exp в SFT BackwardLoss. Старые документы/experiments не удаляются.

Уточнение provenance: raw RESTORE существовал уже наdd150aa, commit81141 добавил отчёт. Donor weights/optimizer сохранены наstep1000 до200continuation updates, а не наstep1200; это подтверждается Program.cs94–97/126–128 и raw. Сохранение результата не объявляет его готовым совместимым S0 для другой топологии.
