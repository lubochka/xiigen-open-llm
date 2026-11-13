# Обучить один раз: каталог общего состояния и его потребителей

## Актуальные пути после core0cf3634

Контракт33 уже разрешил проектный конфликт EA-02. Для исполнения приоритет имеют [canonical mapping](evidence/current-shared-donors-0cf3634.json) и [защищённый контракт33](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/docs/plans/replanning-possible-diffs/experimets/33-shared-learned-starts.md), а не прежние host-local paths ниже. Все перечисленные полные bundles имеют статус **FUTURE_NOT_MATERIALIZED**. Их наличие и payload hashes этим аудитом не выдумываются.

| Прежнее семейство в аудите | Единственный canonical family | Producer и consumers |
|---|---|---|
| SFT-DELTA-JOINT-LF | `h512-dt-joint/v1` | MS01 → MS01/MC01 |
| DPO-TOKENIZER-EXACT-PAIR | `h512-dt-segmented/v1` | TS01 → TS01/KGB/NH01; KGB сохраняет свой provenance, сравнивается model-facing material |
| DPO-SINGLE-POSTFIX | `h512-postfix-one/v1` | AGC1 → AGC1/FBM1 |
| H64-CODE-SIGN-RD01 | `h64-rms-row/v1` | RD01 original A → RD01; наш добавочный план назначает тот же bundle также MB01/AC01 |

Полный префикс каждого пути: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/`. Четыре native файла — weights/optimizer/RNG/preparation — дополнены `generation.json`, `tokenizer.json`, `data.source.jsonl`, `data.material.json`, `authority.json`, `source-manifest.json`, `bundle.json`. `bundle.json` с COMPLETE и SHA/length остальных10 файлов записывается последним. Consumer выполняет проверку/restore без добавочного Forward; назначенные A0/B0 уже входят в его24 forward.

Для новых MB01/AC01 единственный путь: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1`. Private RNG identities — lowercase SHA256 UTF-8 безBOM/конечногоLF: topology literal `RD01|V256|H64|L1|Q4|KV2|FF176|C64|R4|q_proj,v_proj`, LoRA literal `RD01|alpha8|dropout0|seed62002|rms-row-reduce-probe-v1`. Native AdamW step400/m/v и настоящий RNG обязательны; после4 updates каждого arm —404. Старый `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RD01/a001/results/common` используется только назначенным Export-Existing как original source. Consumer не выполняет Export/Prepare самостоятельно.

LC-REF-CACHE уже имеет пригодные qualified weights и завершённые A/B raw. Его `h512-ref-two` **не** готовый полный SFT bundle: исторические moments/RNG не захвачены. Ограниченно допустим свежий DPO optimizer0 и одинаковая fresh DPO RNG lineage, но не подстановка в SFT continuation. Повторного обучения для достраивания комплекта нет. [Числа и custody](09-late-core-0cf3634-update.md).

Ниже сохранено исходное предложение43e2337 как доказательство найденного конфликта. Host-local paths первых четырёх нейронных семейств — теперь исторические original export sources, не альтернативные адреса consumer; назначение FSR01 native kNN остаётся прежним. Данные, qualification и различия SFT/DPO сохраняются; новые canonical paths и COMPLETE contract выше имеют приоритет.

Повторная подготовка общего состояния сейчас назначена нескольким карточкам с одинаковыми данными и carrier. Ниже — конкретный добавочный контракт повторного использования. Это документное изменение планирования, не утверждение о наличии всех бинарных файлов и не выполненное сокращение стоимости.

Источники и полные identity gates: [аудит EA-02](02-current-card-audit.md), [baseline-family-mapping.json](evidence/baseline-family-mapping.json). Существующие карточки в core остаются неизменными; автор будущего исполнения применяет этот явный overlay до выдачи consumer. При несовместимости ничего не переучивать по инициативе исполнителя.

## 1. Назначенные общие состояния

Все пути ниже абсолютны в назначенной Windows-среде будущего исполнения. Под «готовностью» понимается receipt с доказанным обучением и проверенные payload, а не существование каталога.

| Семейство | Producer → consumers | Единственный каталог S0 | Текущий статус |
|---|---|---|---|
| SFT-DELTA-JOINT-LF | E-SFT-RESPONSE-MASK → E-SFT-MOMENT-CONTINUITY | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MS01/results/attempt-0001/common/` | PROPOSED_MAPPING_DONOR_NOT_ATTESTED |
| DPO-TOKENIZER-EXACT-PAIR | DPO-TOKENIZER-SNAPSHOT-01 → N-HEAD-01 | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/results/attempt-0001/common/` | PROPOSED_MAPPING_DONOR_NOT_ATTESTED |
| DPO-SINGLE-POSTFIX | GRAD-ACCUM-COPY-01 → FROZEN-BROADCAST-MAP-01 | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AGC1/results/common/` | PROPOSED_MAPPING_DONOR_NOT_ATTESTED |
| H64-CODE-SIGN-RD01 | Уже назначенная common preparation RMS-ROW-REDUCE-01 → новые RMS-MEAN-BACKWARD-01 и ATTN-SOFTMAX-COPY-01 | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RD01/a001/results/common/` | DEPENDS_ON_QUALIFIED_S0; preparation в защищённой карточке ещё NOT_RUN |
| CSHARP-FILE-ACTION-6 | Один common Train6 в FILE-STATIC-RECEIVER-01 → A0/B0 того же опыта | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/FSR01/a001/results/common/` | DESIGNED_NOT_IMPLEMENTED_NOT_RUN |

У первых четырёх семейства используются ровно `S0.weights.bin`, `S0.optimizer.bin`, `S0.rng.json`, `preparation.json`. Их SHA будущих payload остаются **null**, пока producer не записал и не подтвердил bytes. У kNN состояние иное: `memory.json` с ordered native feature arrays/labels, плюс `preparation.json`; не притворяться, что у FileActionKnn есть native neural checkpoint API.

## 2. Данные и квалификация зафиксированы автором

| Семейство | Неизменяемые данные / carrier | Условие обученного «до» |
|---|---|---|
| SFT-DELTA-JOINT-LF | D-T1/D-T2 chosen, joint `Prompt+"\n"+Chosen`,500+500 предъявлений; embedded JSONL610bytes SHA `efedf046d90e69b3eda5f5200c412ae200a28917396147683b94278c89d58573` | 2/2 полных TRAIN responses exact вS0, хотя бы1 был неверным вW0 |
| DPO-TOKENIZER-EXACT-PAIR | `dpo-tokenizer-snapshot.jsonl`, SHA `91f9409b03dce081e9cd8f431f52fa4157715c44c9d8d22d561fe87fd79a8a95`; materialized chosen IDs/mask без добавленного LF;500+500 | Хотя бы1 заранее назначенный полный TRAIN response W0 false→S0 true; показать обе строки |
| DPO-SINGLE-POSTFIX | `csharp-one-pair.jsonl`, SHA `5b265faaa2046a49449de0f5a44423a28df65bcf79b2c5fabc55e7d6796ba1f6`; `simulation-csharp-doc-f06-008`,1000 предъявлений | Единственный полный TRAIN response W0 false→S0 true |
| H64-CODE-SIGN-RD01 | T1 `C# result sign: 9-4 =`→`+`; T2 `C# result sign: 4-9 =`→`-`; native SFT400, UTF-8 bytes, без newline/BOS/EOS; seeds62001/62002, stream `rms-row-reduce-probe-v1` | `learning_qualified=true`; хотя бы1 полный однотокенный ответ приобретён. 1/2 не превосходит constant sign1/2; вывод ограничен конкретным ответом, не общей арифметикой |
| CSHARP-FILE-ACTION-6 | [Literal fixture](fixtures/file-static-receiver.json), SHA `bc4a70298103d95728c0aa69211cbf5d93ac0dcc3babd7d36b163c43a27d1c69`; первые6 rows в указанном порядке | TrainingSize6, plain3/3 и instance3/3; простой ненулевой total не годится, поскольку empty native default совпал бы с plain class |

Первые три H512 семьи имеют одинаковые model/config/tokenizer/B0 hashes в JSON, но **разные carrier и цель**. Совпадение H512 или `rp01-csharp-small/v1` недостаточно. PAD1/PAD2, ORDER и LC-REF-CACHE не присоединяются к ним: у них другие данные/последовательность/предпосылки. Не объединять все опыты ради красивого одного пути.

Для H64 все36 native source blobs RD01@66947785 и current@43e2337 совпадают: [manifest](evidence/learning-code-source-closure.json). Это доказательство source compatibility, не существования trained bytes. Для остальных групп исходные pins и required-equivalence gates приведены в JSON; их current whole-source compatibility этим проходом не объявлена автоматически пройденной.

## 3. Что именно восстанавливать

Нейронный S0 включает веса/топологию/LoRA, точные tokenizer/carrier, optimizer keyed moments и step, RNG identity/stream/cursor, qualification panel. На consumer — read-only hash/length проверка, restore и повторный export сравниваются с donor; A0/B0 должны совпасть. Результаты consumer не пишутся обратно в producer.

В MASK/MOMENT импортируется SFT AdamW m/v/step1000. В TS01/NH01 и AGC1/FBM1 policy получает S0, независимый frozen reference остаётся B0, **DPO optimizer начинается с0**: SFT moments нельзя выдавать за DPO parent. У новых H64 опытов импортируется настоящий SFT step400 и после4 updates/arm становится404. Новый Trainer counter0 допустим здесь из-за постоянного LR; это не сброс AdamW.

При отсутствии/плохой квалификации/несовпадении source или state — `BLOCKED_DONOR_NOT_READY` либо конкретный identity mismatch **до** учебных шагов. Не искать другой checkpoint, не менять seed, не выполнять400/1000 дополнительных updates, не переименовывать output folder, не подменять restore новым построением той же модели.

## 4. Уже сохранённые состояния: полезность и предел

- **RESTORE-MINIMAL-SET:** raw подтверждает bit-exact весовой restore с hash `db44f1a437653299b345dc83c85523aad1ba1c55d3285865fe92501e5e78e386` и AdamW1000. Но TRAIN всего3/40 правильных токенов и0/2 полных ответов. Это частичное token-level state; оно не заменяет qualified whole-answer donor. [Raw](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/restore-minimal-set.json).
- **PA01 accepted-parent handoff:** SFT-EXP-REUSE-01 уже назначает consumer `PA01/repos/B/.../rp01-ingress-h16/v1/checkpoints/000002` и handoff receipt; это не новый пробел. Наличие назначенного пути не равно завершённой adoption. H16 нельзя подставлять в H512 либо H64. [Точный существующий контракт](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/docs/plans/replanning-possible-diffs/experimets/cards/SFT-EXP-REUSE-01.md#L72-L96).
- **N63C-PROBE:** отчёт указывает `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/results/authority-acquired.bin`; raw подтверждает22/23 tokens, exact=false. Этот путь полезен для provenance частичного результата. Полная state custody/historical binary parity не доказаны; он не выдаётся новым карточкам как готовый23/23 donor. [Отчёт](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/experiments-v2/RP01-MICRO/N63C-PROBE/report.md), [уточнённая граница](02-current-card-audit.md).

Плановое исключение трёх повторных1000-update подготовок может убрать до3000 повторно назначенных SFT updates при успешной совместимости и квалификации. Это арифметика предлагаемого расписания, не измеренная экономия времени и не разрешение на запуск producer сейчас.
