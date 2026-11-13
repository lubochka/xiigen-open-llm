# Актуализация перед публикацией: новый N63C-PROBE, исправленные правила и дедупликация

**Срез этой актуализации core — `256f603e70032c11b1f4ffabbdd1fefdab642ea5`; planning — `8346dfb697be43041a477886db26c508453fe5ce`.** Эти изменения появились после исходного разбора d962/fc0dd. Старые документы 02/03 сохранены как история доказательств; этот раздел определяет текущий статус. Новых runtime-действий мы не выполняли. Последующая delta003 уточняет только документные правила; её текущий wait-контракт — в разделе5 и [отдельной сверке](reviews/latest-wait-reconciliation.md).

Сверены 253 изменённых core path и 40 добавленных planning path; это учёт diff, не 293 смысловых ревью. Прочитаны новые primary source/raw/policy и затронутые карточки; все 26 добавленных planning Markdown получены для адресной дедупликации. Нативные TensorOps, Trainer, LoRA, DPO material и POS методы наших source-факторов в этой core-delta не менялись. [Core compare][core-compare], [planning compare][planning-compare].

## 1. N63C-PROBE действительно дал полезный учебный результат

Это **другой сохранённый запуск**, не достроенное B исходного N63C. В полном [raw][probe-raw] находятся 600 SFT updates приобретения, 1 DPO + 100 SFT в каждом плече: **802 updates и 60 scoring forwards**. У исходного N63C остаются A:1 DPO+1 SFT, B:NOT_RUN и failure при RNG; эти бюджеты не складываются в одну будто бы старую карточку.

Здесь W0 — необученная база, W_auth — после приобретения правильного T1, S1 — после одного исправляющего DPO. A затем получает старый rejected-текст, B — исправленный chosen-текст.

| Показатель | W0 | W_auth | S1, одинаковое наблюдение A/B | После SFT A | После SFT B |
|---|---:|---:|---:|---:|---:|
| T1 правильных response-токенов / 23 | 0 | 22 | 22 | 20 | **21** |
| T1 chosen NLL | 7.307878 | 1.1099185 | 1.1079743 | 1.1158044 | **1.1042708** |
| T1 margin chosen−rejected | 0.2279687 | 1.1063861 | 1.1252952 | **−0.119724035** | **1.1151558** |
| T1 полный teacher-forced ответ | false | false | false | false | false |
| Q1 правильных токенов / 29 | 0 | 0 | 0 | 0 | 0 |
| Q2 правильных токенов / 21 | 0 | 0 | 0 | 0 | 0 |

**Сохранить как успех по измеренным осям:** B удержал на один T1-токен больше A, имеет NLL ниже на 0.0115336 и margin выше на 1.234879835. A перевернул предпочтение в пользу старого rejected. Это полезный узкий результат обучения на конкретном C# материале, а не только статистика веса или loss.

**Ухудшения рядом:** относительно S1 оба плеча потеряли правильные T1-токены: 22→20 и 22→21. Q1 NLL после A=6.987412, после B=7.074539: B хуже на 0.087127. Q2 NLL после A=7.2149773, после B=7.250893: B хуже на 0.0359157. Полных ответов и правильных EXAM-токенов нет; свободная генерация не измерялась. Результат не доказывает обобщение либо сохранение полного ответа. Это не отменяет полезные частичные улучшения.

Оба плеча по коду восстанавливают одни weights, но acquisition optimizer moments не продолжаются: создаются новые оптимизаторы. Совпавшие after-DPO панели подтверждают совпадение наблюдений, не побайтовую тождественность всех hidden states. Времена/allocations сохранены в raw; один фиксированный порядок A→B не доказывает устойчивое ускорение.

В core уже появились [candidate][candidate] и запись [required-changes:142–151][required]. Статус — **ALREADY_DOCUMENTED_CANDIDATE_PRESERVE**. Точный предложенный оригинальный адрес: `14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs`, подготовка `domainExamples` до binding; замена передаваемого SFT.Response при совпадении domain/prompt и точном равенстве rejected, с сохранением SourceIds. Изолированный probe не доказывает достижимость этой коллизии в production; изменение только cache без изменения реально переданного массива её не исправит. Перенос решает пользователь.

## 2. Что уже исправлено документно

| Находка на d962 | Что произошло на 256 | Что остаётся |
|---|---|---|
| N63C ошибочно DESIGNED_NOT_RUN | Current card и 0ea contract теперь consumed / PARTIAL_FAILED / learning objective OPEN; из future outputs удалён | Старый entry не повторяется; полный ответ и A/B исходного опыта не появились |
| RNG filename несовместим с native writer | В current N63 card назначено canonical имя в отдельном sft-state | Это правка рецепта. Исторический Bridge/raw не превращён в успешный Save/Load |
| Поздняя ошибка обнуляет completed counters | Правило независимого stage-учёта добавлено в card и protocol30 | Старый raw сохраняется с erratum; уже сделанные updates не равны нулю |
| 16 явных unqualified-заголовков | 15 заменены на DESIGNED_REQUIRES_LEARNING_VALIDATION; N63C получил consumed/open | Конкретные старые бюджеты в телах 15 рецептов сохранены. Guard исправлен, готовность ненулевого before не доказана |
| Разрешалось сначала израсходовать слабый опыт, затем оформить 0→0 | Новый protocol30 требует заранее заданной единицы усвоения, before/набора и достаточного конечного бюджета | Не выдавать неизменённые два/четыре cold updates как уже квалифицированный опыт |
| Исполнитель не дожидался результата | На256 protocol30 уточнил ожидание; поздний003 protocol31 исправляет двусмысленное правило «два»: быстрые snapshot → настоящий wait; normal silent/running wait продолжается | Диагностика при неисправном wait/противоречии; исходный deadline сохраняется; UNKNOWN не разрешает blind retry |

Основные current источники: [policy30][policy30], [learned-start29][policy29], [N63 card][n63card], [machine contract][contract]. Мы эти файлы не изменяли.

**Оставшиеся редакционные конфликты:** индекс в начале ещё содержит старое «9/38», ниже уже «10/37»; строки и machine contract поддерживают 10/37. В required-changes фраза «пункт 7 карточки неисполним» относится к исторически выданной версии — нынешний пункт 7 уже исправлен. Эти фразы нужно читать с явной датой/версией; они не повод запускать модель или переписывать raw.

В current очереди после corrected08 всё ещё N63C, но действие при достижении — **ремонт открытой постановки**, не повтор consumed entry. Переход дальше — по прямому NEXT либо разрешённой независимой работе. Поэтому прежнее предложение нашего d962-overlay автоматически пропустить N63C заменено этим точным current контрактом. [Актуальный реестр](04-registry.md).

## 3. Граница воспроизводимости нового probe

Completed raw использовал **600 acquisition, 100 SFT/arm, acquire LR=0.0001**. [Source на том же cutoff:54–58][probe-source] уже задаёт **60 acquisition, 20 SFT/arm, LR=0.003**. Текущая команда и текущий source не являются точным воспроизведением completed result. Нужен архив source/build именно той попытки; новый запуск ради оформления его не заменяет. Этот разрыв уже отмечен одновременно опубликованным другим аудитом и не присваивается как наша новая идея.

Отдельный `results-02/progress.log` подтверждает минимум 60 acquisition + 1 DPO + 10 SFT, но завершённый raw для него не установлен. `cycle.log` относится к третьему `results-03` и заканчивается BUILD_BLOCKED. Три каталога не объединяются в один результат; ни один из этих фактов не позволяет утверждать, что unknown процесс «точно не работал».

Новая конкретная проверяемая связь с качеством постановки: acquisition/arm SFT используют LF, а их учебный readout — exact DPO carrier без LF. Общая семья P07 известна; актуальная привязка к current probe и предел объяснения 22/23 разобраны [отдельно](09-n63c-framing.md). Это фактор самого обучающего входа, а не посторонний отчётный тест.

## 4. Согласование с другим planning-пакетом

В planning8346 уже опубликован [SILU-SIGMOID-REUSE-01][canonical-silu], фактически тот же denominator factor, который наш первоначальный черновик назвал SILU-DENOM-REUSE-01. Самостоятельная новизна снята; наш файл оставлен как [review alias](cards/SILU-DENOM-REUSE-01.md), без второго host/ID/запуска. Нельзя считать разные названия разными опытами.

Кроме того, там уже есть [HNSW-QUERY-NORM-REUSE-01][canonical-hnsw] с настоящим Train→Add. Он сохраняется в общем реестре. **Новая самостоятельная гипотеза этого пакета — POS-QUERY-SET-ONCE-01**: другой learner, другой метод, reuse bigram set при чтении обученной POS-памяти. Она не дублирует cosine norm при построении HNSW и идёт последней.

Итого объединённый каталог: **47 core ID + 2 уже опубликованных SiLU/HNSW + 1 новый POS = 50 canonical ID**. Это не 50 запусков, не 50 готовых hosts и не 50 успехов. Из 47 исходных ID 10 имеют сохранённые/частичные attempts; отдельные probes считаются по своим receipts.

## 5. Последняя delta003: ожидание исправлено адресно

[Commit003 compare][wait-compare] изменяет 229 путей, все внутри защищённой документации. Это счёт diff, не 229 новых смысловых ревью. По пяти адресно прочитанным документам порядок и статусы 47 ID, 10 consumed / 37 future, PA01 первым и открытая N63C остались прежними. Source и raw исследуемых механизмов не менялись.

[Protocol31][policy31] требует discovery фактических launch/wait/status и чтения схемы до запуска. Два **быстрых неблокирующих** snapshot без прогресса требуют перейти в настоящий wait. Нормальные bounded wait, возвращающие silent/running, продолжаются до terminal или первоначального deadline без лимита «два». Независимая диагностика нужна при неисправном wait/противоречии каналов. Yield/RPC timeout не равен deadline опыта и не разрешает ни повтор, ни преждевременное завершение живого процесса. Наши активные карточки, prompts и реестр приведены к этому уточнению; ранний review сохранён как срез256.

Имена tools зависят от среды. Указанный в чужом protocol31 `clock.sleep` не подтверждён в callable-каталоге нашей сессии; он не превращён в обязательную команду наших карточек. Исполнитель использует реальные схемы доступных launch/wait, а не выдуманный инструмент. [Подробная сверка](reviews/latest-wait-reconciliation.md), [точный readback пяти файлов](data/latest-wait-readback.json).

[core-compare]: https://github.com/lubochka/llm_mvp_core/compare/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19...256f603e70032c11b1f4ffabbdd1fefdab642ea5
[planning-compare]: https://github.com/lubochka/xiigen-planning/compare/fc0dd1b04d61a963906cdc1ab29dffd2ee9e520c...8346dfb697be43041a477886db26c508453fe5ce
[probe-raw]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/experiments-v2/RP01-MICRO/N63C-PROBE/results/n63c-probe.json
[probe-source]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs#L54-L58
[candidate]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/experiments-v2/RP01-MICRO/N63C/results/candidate.md
[required]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/required-changes.md#L142-L151
[policy30]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/30-failure-recovery-and-learning-validity.md
[policy29]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/29-learned-start-and-parent-continuation.md
[n63card]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md
[contract]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/2026-10-20-card-reconciliation/0ea-execution-contract.json
[canonical-silu]: ../2026-10-20-learning-causal-audit-fb9175cc/cards/SILU-SIGMOID-REUSE-01.md
[canonical-hnsw]: ../2026-10-20-learning-causal-audit-fb9175cc/cards/HNSW-QUERY-NORM-REUSE-01.md

[wait-compare]: https://github.com/lubochka/llm_mvp_core/compare/256f603e70032c11b1f4ffabbdd1fefdab642ea5...003083feab72157e54184bb66d0d940f48d836fd
[policy31]: https://github.com/lubochka/llm_mvp_core/blob/003083feab72157e54184bb66d0d940f48d836fd/docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md
