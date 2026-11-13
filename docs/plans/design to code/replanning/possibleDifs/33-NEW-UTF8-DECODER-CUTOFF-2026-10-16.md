# N33-02 — Неполный UTF-8 символ на границе генерации: отдельный путь потери качества EN/RU/HE/ES

<!-- EFFECTIVE-AUDIT-2026-11-02-POST102:START -->
## POST102 — сохраняется именно исходный byte stream, а смысл оценивается отдельно

**NAMED_CHECK: POST102-UTF8-SUFFIX-SCOPE, 02.11.2026.** Уточняет приведённый ниже результат P76 и исторический Gate1. Удаление незавершённого суффикса в P76 и предложение догенерировать символ — разные вмешательства. [Аудит POST102](2026-11-02-POST102-TRANSFER-AND-EXECUTOR-AUDIT.md) не назначает второе вмешательство или новый бюджет; прежнее уменьшение replacement 5→2 при full-label 0/4 остаётся узким результатом декодера.

**WRONG:** перекодировать уже испорченную строку и назвать полученные ID исходной генерацией; одновременно увеличить cap и trim suffix; уменьшение «�» назвать улучшением перевода. **CORRECT:** A/B получает один сохранённый массив фактически выбранных IDs и один byte stream из того же vocabulary; меняется только назначенное правило границы. Сохраняются исходные bytes, stop reason, число/позиции удалённых bytes и результат strict UTF-8. Если полных actual IDs/bytes нет, происхождение хвоста NOT_MEASURED: `Encode(Decode(ids))` не восстанавливает потерянные решения генератора.

**Проверка Gate0 одним callable:** тот же decoder/validator вызывается для эталонов и обоих плеч. Полные ASCII и `D0 BF` («п») сохраняются; незавершённый `D0` обнаруживается как хвост; внутреннее повреждение `FF 61` не превращается в «валидный ответ» удалением произвольных bytes. Неизвестный token ID обозначается отдельно от незавершённого UTF-8 и не исчезает молча. Порча проверяет реальный callable, а не вручную изменённый флаг. Предметный scorer отдельно проверяет сохранность числа, отрицания и ролей; правильное начало с противоречащим хвостом не проходит.

**Предел и адресное продолжение:** владелец N33 сначала проверяет доступные P76 bytes и контрольные случаи без model/Train calls. Suffix trim не доказывает полноценный перевод, EOS, работоспособность внутреннего повреждённого потока или пользу на всех языках. Догоняющая генерация требует отдельной проверки того, какие новые токены и затраты она вносит; её нельзя выдавать за бесплатное чтение прежних bytes. Python byte proxy, native decoder и обычный consumer сохраняют разные статусы доказательства.
<!-- EFFECTIVE-AUDIT-2026-11-02-POST102:END -->

<!-- EFFECTIVE-AUDIT-2026-11-01:START -->
## Актуализация доказательств 01.11.2026 — P76

P7601 уже выполнен:400SFT updates подготовили модель, decoder сравнен на одних frozen bytes; replacement5→2, removed3bytes, strict-valid0/4→2/4. После назначенного crop full-label0/4 оба. Добавить valid complete UTF8/ASCII и interior FF61 controls: trim только незавершённого suffix, без сокрытия внутреннего повреждения. Не переносить этот узкий decoder gain на качество всех EN/RU/HE/ES ответов.

Основание: [2026-11-01-TRANSFER-EXECUTION-AND-EXPERIMENT-AUDIT.md](2026-11-01-TRANSFER-EXECUTION-AND-EXPERIMENT-AUDIT.md), закреплённый core `790228c256168a336b01abae4387e28391048c0c`. Старый текст и его ограничения относятся к указанной там дате; новый блок уточняет текущий статус, не переисполняет исторический опыт и не меняет научную очередь.
<!-- EFFECTIVE-AUDIT-2026-11-01:END -->

**16.10.2026 | SOURCE-VERIFIED + EXECUTED PYTHON BYTE PROXY / NATIVE NOT RUN / ACTUAL QUALITY IMPACT UNPROVEN**

Core: llm_mvp_core codex/dpo-implementation @ 32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7. Отличается от известного [EOS failure](10-INFERENCE-STOP-TOKENS-AND-UNIT-TRANSFER.md), [необученного end-of-answer target](26-INDEPENDENT-STOP-TARGET-AND-SAMPLER-2026-10-16.md), [N28 Unicode normalization](28-NEW-UNICODE-CANONICAL-KEY-AND-SPANISH-N-FALSE-MOJIBAKE-2026-10-16.md) и [N23 неизвестных token ID](23-INDEPENDENT-AUDIT-SHAPE-DECODER-AND-LEARNING-2026-10-16.md). Это **прерывание валидного byte-stream** на временной/token границе, не ошибка кодирования исходного текста и не неизвестный ID.

## Для человека

Byte-level BPE допускает, что модель выдаёт один Unicode символ **несколькими byte tokens**. Генератор прекращает выдачу по maxNewTokens (или случайному EOS ID) и одним вызовом Decode переводит получившийся byte stream в строку. Если граница пришлась после первого байта русской «п», но до второго, стандартный UTF-8 decoder заменяет неполный символ знаком «�». Получается невалидный перевод **без какого-либо изменения LoRA или реального смыслового знания**.

Это условная гипотеза о потере качества именно при данном поведении модели/бюджете. В текущем RP01 EN→RU500 нет доказанной связи с его **0/4→0/4**; он использовал maxNewTokens 256 в базовом manifest, нужно реально измерить stop reason и raw generated token IDs каждого проваленного ответа. ASCII-only C# позволяет отрицательный контроль.

## Source evidence

1. [BpeTokenizer.Train:42–68](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/01.Tokenizer/BpeTokenizer.cs#L42-L68) формирует base byte vocabulary 0..255, далее добавляет merges. Значит некрупный byte fragment может быть самостоятельным valid ID.
2. [Encode/Decode:73–92](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/01.Tokenizer/BpeTokenizer.cs#L73-L92) соединяет byte[] всех токенов, затем применяет Encoding.UTF8.GetString(bytes). Никакого правила "отложить незавершённую UTF-8 последовательность и продолжить генерацию" здесь нет.
3. [TextGenerator.GenerateCore:212–250](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/09.Inference/TextGenerator.cs#L212-L250) добавляет nextToken, сразу останавливается по EOS/maxNewTokens, затем Decode(generated). StopReason возвращает EOS/LENGTH_LIMIT, но не индикатор нарушенного Unicode byte boundary.
4. [RP01 config](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/config/code-documentation/generation-v1.json#L26-L48): normal greedy temp 0, maxNewTokens=256, LoRA dropout=0. Никакой stochastic sampler здесь автоматически не виноват.

## Выполненный короткий proxy — статус ровно byte-level

Source-equivalent Python вызов bytes.decode("utf-8", errors="replace") выполнен для 4 валидных двухбайтовых символов. Сначала был получен только первый byte token, затем оба. Результат [proxy receipt](33-MICRO-UTF8-PROXY-RESULT-2026-10-16.json):

| Язык/символ | После первого byte ID | После второго byte ID |
| --- | --- | --- |
| RU «п» (D0 BF) | � | п |
| RU «я» (D1 8F) | � | я |
| ES «ñ» (C3 B1) | � | ñ |
| HE «א» (D7 90) | � | א |

**Что доказано:** неполная UTF-8 последовательность при replacement decode даёт искажение; кодовая цепь может остановить выдачу до полного символа. **Не доказано:** что реальные Student token IDs оканчиваются неполным символом или что качество RP01 упало из-за этого. Python proxy не подменяет native .NET либо blind CODEX evaluation.

## Micro N33-UTF: как отличить от недоученной модели

**Gate 0:** на frozen checkpoint/source-disjoint heldout 8–16 EN→RU/HE/ES prompts сохранить в native TestGenerator raw token IDs, final bytes (обратное склеивание vocab bytes), stop reason, byte-UTF8 strict decoder success/failure, actual string, law correctness до оценки. Повторить 3 ASCII C# cases для отрицательного контроля. Если все final byte sequences валидны, условная гипотеза **FALSIFIED_FOR_THIS_RUN**.

**Gate 1, one-factor A/B:** не обучать модель, не менять sample temp/k/seed/tokenizer. Для совпадающих frozen logits/forced first tokens A: stop at N bytes/tokens; B: только разрешить завершить текущий UTF-8 scalar (максимум +3 bytes, при сохранении byte-token framing), не добавляя целое следующее слово и не переписывая gold. Для каждой пары сохранить raw token IDs, original stop, corrected validity и CODEX/Astra 6 UltraCode independent semantic correctness. Исключить примеры, где B меняет не только хвост символа. Проверить, что canonical decoder и формат выходного ответа допускают boundary completion; иначе просто diagnostic без внедрения.

**Метрики:** invalid UTF-8 fraction by stop reason, replacement-char count, semantic law accuracy per domain, wrong→right vs right→wrong, extra tokens and latency, restored model parity. Не считать уменьшение «�» само по себе доказательством лучшего перевода. Для C# ASCII прогноз 0 effect. Если ни одного native malformed tail, N33-02 не объясняет отрицательный ENRU RP01 и задача закрывается без производственного изменения.

См. [N33 MICRO executor](33-MICRO-CAUSAL-EXECUTOR-AND-EXPERIMENT-2026-10-16.md). Статус результата: **SHADOW_PASS_FOR_BYTE_BEHAVIOUR / NATIVE_NOT_RUN / SEMANTIC_QUALITY_UNKNOWN**.
