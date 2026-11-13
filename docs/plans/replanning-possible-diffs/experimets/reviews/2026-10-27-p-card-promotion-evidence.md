# P01–P125: основания документного повышения, 27 октября 2026

Фаза — READ_ONLY_RESEARCH и изменения Markdown. Worker `/root/transfer_math_audit` прочитал каталог 125 P-файлов, их заголовки, выбранные подслучаи A/B и адресные исходники; это не 125 независимых code review. Runtime, сборки, тесты, обучение и inference не запускались. Исходный snapshot этих постановок — `6e58d068db4de36b82afff49fd8a8f98eba8eb9c`; при завершении worker прочитал уже `dc8eb29bd3a8fa0429d0263cb056799b3ae25f25` в совместной рабочей копии. Полный аудит различий этих commits не заявляется. Прежний transfer audit был закреплён за `4ce8bdd0dd3942a20b32c569c2b0f0c37894dd23` и не подменяет эти снимки.

## Непересекающаяся карта всех 125

| Группа | P-номера | Число | Настоящий получатель |
|---|---|---:|---|
| A — neural/DPO/serving | 1,2,4,5,6,7,8,9,10,11,12,13,15,24,68,75,76,78,81,86,87,89,98,99,100,101,102,105,114,119,121 | 31 | LlmModel/DpoTrainer/Trainer, обученный tokenizer, либо потребитель сохранённого обученного state |
| B — evidence/evaluation | 3,14,57,66,71,77,79,80,82,85,91,93,109,112 | 14 | Тот же настоящий learner и его данные, admission, scorer или learned-output consumer |
| C — Unit/retrieval | 16,17,18,19,20,21,22,23,25,29,30,31,32,69,74,90,107,108,110,113,115,122 | 22 | Обучаемые memory/index и обычный Execute/Flow |
| D — routing | 33,34,35,36,37,38,39,40,41,42,72,73 | 12 | Обученный router и фактический route consumer |
| E — алгоритмы | 26,27,44,45,46,47,48,49,50,83,84,116,120 | 13 | Конкретный нативный статистический learner |
| F — translation | 28,43,51,52,53,54,55,56,58,59,60,61,62,63,64,65,67,104,106,117,125 | 21 | TranslationMachine и обучаемые Unit, не произвольная LoRA |
| G — orchestration | 88,92,94,95,96,97,111 | 7 | Реальный capability/Flow bridge с отдельным контрактом |
| H — product consumers | 70,103,118,123,124 | 5 | Названный конечный consumer; отсутствие learner не скрывается фиктивным SFT |

Сумма и уникальность этой карты проверены арифметически: 125 различных P-номеров, пропусков 1–125 нет. Это инвентаризация, не выполненные эксперименты. Первоначально worker владел 45 карточками A+B. Затем parent передал ещё P88, P95, P97, P123, P124 и DPO-TERMINAL-TARGET-QUALIFIED-02: итог — 50 P-паспортов и один отдельный квалификационный паспорт; parent владеет остальными 75 P, общим договором и индексом.

## Существующая первичная evidence

- [P11 component report](../results/2026-10-18/P11-embedding-bias-01/report.md): нативный autograd и controlled finite differences, без обучения модели. [P11](../cards/P11-autograd-reachability.md) дополнительно ссылается на сохранённый `embedding-sft8-01`; компонентный результат нельзя выдавать за heldout quality.
- [P86a report](../results/2026-10-18/P86a-mask-01/report.md): нативные score/backward при дробных mask, zero training; окончательный Release result отличать от предварительной ошибки Debug. Git `a392b6be8f8` затем исправлял embedding/bias/weighted DPO. Нынешний код не является автоматически историческим A.
- [P125 report](../results/2026-10-18/P125-enru-memory-01/report.md): реальное обучение translation memory и ordinary Flow, 16 lessons суммарно для B/C, 144 Flow calls; B 0/24→19/24 strict. Это сохранённый результат, а не ещё одна постановка на выполнение.
- [P25 report](../results/2026-10-18/P25-enru-cold-01/report.md): 0 новых Train, одна загрузка P125 B state, 24 ordinary Flow; parity 24/24 и strict 19/24. Исходный state SHA256 `125c15ddb400dc00cfddb2367e1ea811917a87fb641c4e58ade22da2bea2e56c`.
- [h64 learned donor](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1/bundle.json): COMPLETE, 400 updates, приобретён только T1. T2 не превращается в доказанный навык. Bundle закрепляет веса, optimizer, RNG, tokenizer и материал; новый PRIMARY не переиспользует старое mutation permission.

Поиск P-prefixed артефактов в ограниченных корнях и Git истории не доказывает отсутствие alias-run. Для каждого нового PRIMARY статус будет `DESIGNED / SHALL_IMPLEMENT`; это утверждение о новом назначенном ID/host, не «вся P-семья никогда не запускалась». Существующие результаты сохраняются и учитываются до будущего запуска.

## Зависимости, которые нельзя спрятать в шаблоне

1. `DpoProjectLoraRecipient` ограничивает фактический trainable recipient. P01 rank/alpha допустимо назначить конкретно; trainable lm_head требует отдельного bridge/allowlist и не предполагается работающим по наличию Linear.
2. `02.DatasetPipeline/DatasetBuilder.cs::BuildAsync` пишет token stream+EOF; stride находится в `DataLoader.NextBatch` (`_pos++`). P68 должен менять этот caller, сохраняя target alignment, а не выдуманный параметр Builder.
3. S0 weights-only не равны continuation. Для DPO/SFT — params, optimizer moments/step, RNG, reference identity; для cache — lifetime/rollback; для memory — state/schema и ordinary consumer. Топология rank4/rank8 требует явно нового zero-adapter старта, не перепаковки обученного rank8.
4. P71/P77/P79/P80/P82/P91 имеют конкретные E-family learners. Новый readout/paired seam описывается как experimental adapter, не как уже существующий production API. Межалфавитный PPL и изменение знаменателя не называются ростом качества модели.
5. P75/P76/P99/P100/P121 относятся к использованию обученного state. Нужен настоящий trained consumer; synthetic boundary — дополнительный контроль, а не подмена его ответов. Нельзя добавлять одинаковый формальный SFT ко всем механизмам.
6. Прежняя карта не устанавливает готовый neural learner для P92/P123/P124: scaffold critic/QA evidence и literal failure discriminator сами по себе не обучаемая система. P123/P124 позднее переданы worker и получили явные новые native-KNN prototypes. P92 остался у parent; его bridge также должен быть явно назначен, а не объявлен найденным.

Новые паспорта следуют [общему договору](../34-P-experiment-execution-contract.md). Они supersede только выбранный PRIMARY recipe; старые источники, остальные подслучаи и результаты остаются видимыми. В текущей фазе создаются только документы, никакие назначенные H/entry/fixtures не реализуются.

## Записанный результат документной работы

Worker `/root/transfer_math_audit` записал PRIMARY в начало следующих 50 живых карточек: P01, P02, P03, P04, P05, P06, P07, P08, P09, P10, P11, P12, P13, P14, P15, P24, P57, P66, P68, P71, P75, P76, P77, P78, P79, P80, P81, P82, P85, P86, P87, P89, P91, P93, P98, P99, P100, P101, P102, P105, P109, P112, P114, P119, P121, P88, P95, P97, P123, P124. У каждой назначены конкретные native writer/reader, TRAIN/EXAM/gold, state, A/B, контроль, конечные расходы, H/R/entry и команда будущего исполнения. На статическом readback прочитаны первые 27 строк каждой карточки: у всех 50 ровно ожидаемый верхний BEGIN/END, нужные восемь полей и соответствующая точная команда. Это авторство и self-readback, не 50 независимых reviews и не 50 запусков. Исторические тела сохранены; в P123.1 адресно исправлена ошибочная ссылка на отсутствующий `occlusionDetected` на настоящее `QaResults[0].Verdict.Passed` при неизменном Score .9.

P01 после замечания parent исправлен: zero-output adapters разных ranks получают по 400 native SFT updates; только при acquired T1 обоих arms допускаются по 4 DPO. Максимум 808 updates, 16 DPO pair presentations, 16 800 SFT next-token targets. Невыученный старт — INVALID_START, размер adapters не становится обходным успехом. P76 сохраняет исходный H64 state и получает одну общую подготовку 400 SFT для новых RU меток; optimizer step 400→800. Перед decoder A/B необходим хотя бы один полный вновь правильно воспроизведённый RU TRAIN ответ; synthetic UTF8 controls не подменяют это условие.

P88, P95, P97 назначают новые узкие KNN/typed-Flow bridges с настоящим native Train и изменением сохранённого corpus. P123 — новый geometry-conditioned KNN с переводом обоих rectangles и swapped-label control, а не зрение по pixels. P124 — новый named-case KNN prototype над literal contract fields; старый P3 BaseService discriminator не переименован обучением. Scope каждой новой постановки отделён от исторической гипотезы.

В [DPO-TERMINAL-TARGET-QUALIFIED-02](../cards/DPO-TERMINAL-TARGET-QUALIFIED-02.md) записан полный TTQ02: существующий `N63C-PROBE/results-03/authority-acquired.bin`, SHA256 `59810def905c4ead2f22985c5e1d525f59feb45590256eca8b492a293206d44f`, и исходный W0. Фактическая конфигурация V629/H512, не V256; назначены 12 frozen Forward = 2 states × 3 строки × 2 framing views, новых Train/SFT/DPO/Generate нет. Exact-DPO framing — primary, joint-LF acquisition view — диагностический. Это будущая квалификация данного donor, не доказательство его пригодности сейчас и не alias проваленного SC02. Идентичности binary/config/tokenizer/fixture были прочитаны с диска; новый host не создавался.

Отдельный адресный review **родительских** P42/P64/P92/P94/P111/P118 и общих 34/35 сохранён в [пакете review](2026-10-27-p-primary-targeted-review.md). Он не считается независимой проверкой собственных 50 паспортов. После исправлений выполнен содержательный readback затронутых верхних блоков; выполнение команд, сборка, numerical qualification и воспроизводимость будущих host не проверялись.
