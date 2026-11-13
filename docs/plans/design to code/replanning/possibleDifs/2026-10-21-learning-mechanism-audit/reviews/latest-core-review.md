# Адресная проверка нового core 256f603: N63C, learning validity и очередь

**Дата:** 2026-10-21. **Режим:** READ_ONLY_RESEARCH, без build/train/test/inference. **Новый SHA:** `256f603e70032c11b1f4ffabbdd1fefdab642ea5`, прямой parent `d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19`. Ни защищённые документы, ни продукт, ни старые evidence-копии не изменены. Новые source-копии находятся только в `latest256/`; 37/37 текстовых Git blobs проверены по SHA, receipt — `latest256/materialization-receipt.json`. LFS pointers проверены как текст Git, сами binary payload не скачаны и не проверены.

## 1. Что меняет вывод

В новом коммите есть завершённый **отдельный** `N63C-PROBE/results/n63c-probe.json`: полезный частичный учебный эффект теперь измерен. Его нельзя приписывать старому N63C, у которого выполнено только A и нет действительного учебного сравнения. Нельзя также объявлять закрытым требование полного ответа: T1 exact остаётся false, EXAM correct-token counts остаются нулевыми. В новом protected `required-changes.md` кандидат уже зарегистрирован, поэтому добавление нашей документации не будет первым обнаружением или первой регистрацией этого кандидата. [Raw:17–195,197–417,596–612][R1], [кандидат:142–151][R2].

Главные ограничения нового результата: текущий исходник задаёт **60/.003/20**, а завершённый raw описывает **600/.0001/100**; не предъявлена точная связка этого raw с исполнявшимися source/build bytes. Сохранённый `results-02` — другая, частичная запись; `cycle.log` относится уже к `results-03` и заканчивается `BUILD_BLOCKED`. Отсутствие финального файла во втором каталоге само по себе не доказывает, что процесс умер или не исполнялся. [Source:49–58][R3], [raw:6–10][R1], [progress:1–17][R4], [cycle:1–6][R5].

## 2. Четыре разных записи исполнения

| Запись | Что действительно сохранено | Учебный статус и предел |
|---|---|---|
| Старый `N63C/results` | A: 1 DPO + 1 SFT, 18 scoring forwards; поздняя ошибка RNG writer; B не выполнено. Новый card это признаёт. | Consumed/partial failed; 0/23 во всех состояниях старой панели. Учебная цель OPEN. |
| `N63C-PROBE/results` | Завершённый JSON: 600 acquisition SFT + 200 arm SFT + 2 DPO = **802 updates**, **60 scoring forwards**, 0 Generate, 632.0084716 s, whole-process peak 1,180,323,840 bytes. | Измерены частичное усвоение T1 и различие удержания/preference. Полное владение ответом и обобщение не показаны. |
| `N63C-PROBE/results-02` | Progress: 60 acquisition, A DPO, A SFT 10/20; нижняя граница **71 выполненный update**. Сохранён другой `authority-acquired.bin` pointer. | Завершение A/B и итоговые counts по имеющемуся срезу не установлены. Не дописывать нули за неизвестные операции. |
| `cycle.log`, root `results-03` | Build blocked в 04:35:32; строка 2 явно называет третий root. | Не финал `results-02`; не результат обучения. Не суммировать с complete raw как единый прогон. |

Ссылки: [card:3–15][R6], [raw:596–612][R1], [progress:1–17][R4], [cycle:1–6][R5]. Первые два пункта не являются количеством новых canonical experiment IDs. Указанные runtime numbers прочитаны из сохранённых свидетельств, не воспроизведены.

Полезный точный предел расходов: `raw.budget.scoringForwards=60` не означает 60 всех model forwards; это счётчик диагностических scores. Включать сюда внутренние training/reference forwards без дополнительного receipt нельзя. Число 802 относится только к complete `results`, не ко всем попыткам.

## 3. Что улучшилось и что ухудшилось

Все числа ниже — teacher-forced T1, а не свободная генерация. В complete raw A/B имеют одинаковые показанные after-DPO панели и DPO metrics.

| Метрика T1 | W0 | W_auth | После DPO, A=B | A stale после SFT | B corrected после SFT |
|---|---:|---:|---:|---:|---:|
| Правильные response tokens | 0/23 | 22/23 | 22/23 | 20/23 | 21/23 |
| Response exact | false | false | false | false | false |
| Chosen NLL, меньше лучше | 7.307878 | 1.1099185 | 1.1079743 | 1.1158044 | 1.1042708 |
| Chosen−rejected mean-logP margin | 0.2279687 | 1.1063861 | 1.1252952 | −0.119724035 | 1.1151558 |

[Raw:197–417][R1]. B сохраняет на один правильный токен больше A и больший preference margin; это полезные измеренные изменения соответствующих метрик C# T1. Одновременно **оба** плеча теряют правильные токены относительно W_auth: −2 и −1. Нельзя описывать B как полностью сохранивший правильный ответ.

Контроли: Q1 во всех шести панелях 0/29, Q2 — 0/21. У Q1 NLL W_auth 6.993638 → A 6.987412 / B 7.074539; у Q2 7.1899514 → A 7.2149773 / B 7.250893. B хуже A по этим NLL, хотя избегает отрицательного Q2 margin: A −0.21515656, B +0.028256893. Все 18 panel rows имеют response exact=false. [Raw:197–389,474–593][R1]. Это частичный полезный кандидат со смежными регрессиями; цель полного ответного поведения остаётся открытой. Полезный margin нельзя обнулить из-за отсутствия прироста full-response accuracy, но нельзя и заменить им заранее назначенную full-response retention цель.

Сохранённый report считает гипотезу подтверждённой; более точная формулировка для нашего документа: **«в сохранённой диагностической пробе исправленный SFT лучше удерживает часть выученных токенов и предпочтение; перенос в оригинальный pipeline — кандидат; достижимость collision в продукте, полный ответ, свободная генерация и executable provenance не установлены»**. [Probe report:93–98][R7], [candidate:11–18][R8].

## 4. Source/result расхождение и восстановление состояния

1. **Complete raw не воспроизводится текущими constants.** Source:54–58 задаёт acquisition LR .003, max60/check10, arm20/LR.003. Raw:7–9 и 18 описывает LR .0001, 600,100; progress complete идёт каждые25. В source:52–53 есть утверждение об обрыве фоновых процессов, но это комментарий автора, а не установленная причина. Не назначать current `Run-Probe.ps1` как команду повторения complete результата.
2. **Слабый acquisition predicate не ограждает intervention.** Source:160–167 запускает оба плеча независимо от `acquiredExact`; :199–201 объявляет ACQUIRED при `acquired > 0`. В новом policy30:36–38 и card:210–216 для retention нужен показанный заранее назначенный навык до вмешательства. Текущий probe может продолжить treatment при неподходящем before. Исправление — будущий авторский gate по конкретной цели, а не повышение steps до PASS исполнителем. [Source][R3], [policy30][R9], [card][R6].
3. **Weights-only fork и общий reset — отдельный design.** Acquisition:155–158 сохраняет weights. Arm:263–267 загружает свежие B0/reference и authority weights; :332–335 создаёт DPO trainer; :350–357 создаёт новый SFT Trainer/AdamW. Acquisition optimizer/RNG continuity этим не доказана. Это допустимо как явно одинаковый reset двух плеч, но не как full continuation обученного состояния. После arm SFT :375 сохраняется weights; полной SFT optimizer/RNG bundle здесь не видно. [Source][R3].
4. **RNG writer-контракт в документах исправлен.** Новый card:130 назначает отдельный `sft-state/dpo_random_state_step_000001.bin` через native BuildFileName; DPO checkpoint остаётся отдельным. В probe observer:316–328 canonical writer уже используется для DPO. Это не переписывает старый упавший N63 bridge и не превращает его partial attempt в завершённый опыт. Protected required-changes:149 всё ещё говорит «пункт 7 неисполним» — обозначать именно **исторически выданный** пункт, потому нынешний :130 исправлен. [Card][R6], [source][R3], [candidate register][R2].
5. **Линия воспроизводимости артефакта ограничена.** Complete authority LFS pointer: sha256 `2ac54968f98e44ee6cec793e755c0775521c4c08d8e2a2285eef5ddc13a7da87`, 47,899,519 bytes; results-02 pointer другой: `59810def905c4ead2f22985c5e1d525f59feb45590256eca8b492a293206d44f`, тот же размер. Нельзя подменять один другим. Pointers подтверждают объявленные identities, не доступность/валидность загруженного payload.

Дополнительный lead, совместно проверяемый root: source:124/357 обучает `Prompt + "\n" + Response`, а :388–396 строит DPO material; SideScore:434–474 читает этот carrier. Native no-separator contract root проверяет отдельно. Это конкурирующее объяснение несоответствия очень малого acquisition loss и устойчивых 22/23 response tokens. В прочитанных новых N63 docs точного объяснения этого расхождения не найдено; причинность, позиция ошибочного токена и самостоятельная новизна здесь не заявляются.

## 5. Что исправил новый policy, а что осталось авторской задачей

Новый `30-failure-recovery-and-learning-validity.md` явно разделяет execution status, learning validity, hypothesis status; поздняя запись не обнуляет updates; unknown=null; 0→0 не закрывает учебную цель. После двух одинаковых пустых observations требуется иной read-only источник состояния **того же** процесса, а не перезапуск; deadline не расширяется. Неизвестный исход не позволяет results-02/retry. При прямом STOP/NEXT переход немедленный; без такого указания невалидный report не закрывает незавершённую учебную цель. [Policy:7–20,26–40,44–64][R9].

Wrapper пока не соответствует будущему этому recovery contract: `Run-Probe.ps1`:16–23 выбирает новый results-02..99; :14/40/45 перезаписывает общие cycle/build/run logs; :25–34 меняет compile-list старого N63C host. Сам dotnet invoke синхронный (:45), что полезно, но запуск wrapper повторно не является наблюдением того же owned процесса. Авторизация исторических повторов по одному snapshot неизвестна — не объявляем её отсутствовавшей; нынешний READ_ONLY scope не разрешает новый запуск. [Wrapper][R10].

### Дельта 16 readiness заголовков

Адресно сравнены все 16 старых явно unqualified карточек, не все 168 карточек повторно. У **15** заголовок заменён на `DESIGNED_REQUIRES_LEARNING_VALIDATION`, добавлены learning/recovery guards. У N63C статус заменён на consumed/open. Это исправление политики допуска, **не** 16 новых подходящих baseline/бюджетов. Старый inventory из 335 документов остаётся историческим срезом d962. Машинный адресный diff: `latest256/unqualified-delta.json`.

Конкретные сохранённые бюджеты: E-DPO-VALID-LENGTH:99 — 4 DPO (2/arm), SFT0; E-SFT-MOMENT-CONTINUITY:99 и E-SFT-RESPONSE-MASK:99 — 4 SFT (2/arm); E-DPO-SFT-ORDER:99 — 2 DPO +4 SFT total. В :63 всех четырёх остаются SFT .001/TotalSteps4 и DPO 1e−5. Ни header rename, ни policy вставка не демонстрируют, что эти числа достаточны для назначенного навыка; автор должен предъявить подходящее before или исправленный конечный acquisition recipe до исполнения. Недостаточность каждого не запущенного рецепта отдельно здесь не доказана.

PA01/corrected08 получили policy-вставки, но прежние H16 partial donor/budgets сохраняются. PA01:84–86,177 честно указывает 3/40, exact0/2; corrected08 остаётся continuation этого принятого step1000, с full-answer objective open. Новый planning H64 sanity evidence проверяет root отдельно: оно не означает автоматически совместимый H512 before для N63C или замену H16 в назначенной карточке. [Learned-start:22–23,44–54][R11].

## 6. Очередь и нужные правки нашего пакета

Машинный core контракт содержит **47 unique queue IDs**, **10 consumed saved/partial attempts**, **37 future_outputs**. Это не 10 успешных учебных задач. Active next — PA01, затем corrected08, затем **исправление постановки N63C**, а не повтор consumed entry. Для N63C `action_on_reach=REPAIR_ASSIGNED_CARD_DESIGN_NO_REISSUE_OF_CONSUMED_ENTRY`; переход дальше требует прямого user NEXT или уже разрешённой независимой работы. [0ea:6–7,81–115,1078,1150–1168][R12].

| Наш раздел | Адресное изменение |
|---|---|
| 02 semantic audit | Оставить d962 как датированную историю; добавить current delta: N63C статус и canonical filename исправлены в документах; старый host/raw не исправлялись; 15 заголовков переименованы, calibrated recipes не предъявлены этим переименованием. |
| 03 N63 amendment | Пометить writer/counts amendment частично уже принятым core256; сохранить необходимость qualified before, stage counts, common reset/continuation и source/result custody. Не предлагать изменить защищённый core. |
| 04 registry/data | Сохранить старые47/10/37 как scope core queue. Исправить автоматический skip N63C после corrected08: сначала документная design repair, прямой NEXT выше неё. Новые planning49 и POS50 root учитывает отдельным additive layer. |
| 05 results/candidates | Добавить complete probe отдельно от старого failed N63C и incomplete results02; показать T1 22→20/21, margins, EXAM регрессии и exact=false. Кандидат уже записан в required-changes. Не приписывать current source complete run. |
| 07 sources/coverage | Добавить новый SHA, 37 exact-text materializations, targeted delta scope; old335 inventory не переименовывать в current full semantic audit. LFS pointers ≠ проверенные payload. |

Самостоятельная новизна SiLU и число canonical IDs изменились в новом planning head; это проверяет root. Этот аудит не подтверждает старое заявление о самостоятельной новизне SILU-DENOM-REUSE-01 и не пересчитывает planning corpus самостоятельно.

## 7. Точные primary references

Все ссылки ниже pinned на core256. Локальные копии с исходной нумерацией строк — в `latest256/<repository path>`. В этом проходе релевантные skills не перечитывались: применены ранее адресно прочитанные phase-planning / plan-review / code-examination / prompt / human-communication / architecture sections; статус — **targeted-read retained**, не «полностью прочитаны все skills».

[R1]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/experiments-v2/RP01-MICRO/N63C-PROBE/results/n63c-probe.json
[R2]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/required-changes.md#L142-L151
[R3]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs
[R4]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/experiments-v2/RP01-MICRO/N63C-PROBE/results-02/progress.log
[R5]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/experiments-v2/RP01-MICRO/N63C-PROBE/cycle.log
[R6]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md
[R7]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/experiments-v2/RP01-MICRO/N63C-PROBE/report.md
[R8]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/experiments-v2/RP01-MICRO/N63C/results/candidate.md
[R9]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/30-failure-recovery-and-learning-validity.md
[R10]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/experiments-v2/RP01-MICRO/N63C-PROBE/Run-Probe.ps1
[R11]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/29-learned-start-and-parent-continuation.md
[R12]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/2026-10-20-card-reconciliation/0ea-execution-contract.json
