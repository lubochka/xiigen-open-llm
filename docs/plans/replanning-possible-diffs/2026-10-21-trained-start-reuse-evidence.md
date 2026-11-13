# Shared learned start: первичные различия и сохранённые доноры

21.10.2026; READ_ONLY_RESEARCH + три назначенных документа. Worker /root/rp01_primary_evidence, без вложенного агента. Новых Train/Forward/build/test/runner/process/Git mutations нет. Это адресный source/readback, не новый эксперимент и не полный аудит исторического корпуса.

## Вывод и применённый документный результат

Одного универсального S0 среди проверенных данных нет. При этом заявление «B0 необучен, значит нигде нечего восстановить» неверно: реальные H16/H32 и H512 probe weights существуют, но различаются shape, TRAIN, carrier, optimizer/provenance и квалификация. PA01 относится к private SFT adoption, а не к canonical product parent. Во вложении [7043…:44–53](C:/Users/lubao/.codex/attachments/7043b77b-0c8b-4df3-a2b7-b8e51a2229e7/Pasted%20text.txt:44) эти границы смешаны; повтор1000 prescribed карточкой не доказывает отсутствие других donors.

Созданы [live contract33](experimets/33-shared-learned-starts.md) и [machine map](reconciliation/shared-learned-starts-2026-10-21.json):12 отдельных family,16consumer mappings, consumer common updates0. Future shared root для каждой — C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/<family>/v1. Prepare/Export interfaces описаны, не материализованы. Матрица не разрешает запуск и не изменяет существующую LC попытку. Parent владеет карточечными/индексными apply.

## Совместимость: точные карточечные различия

| Группа | Проверенное основание | Решение |
|---|---|---|
| LC-REF-CACHE | [N61:30](experimets/cards/N61-dpo-reference-reuse.md:30): f06-008+f21-001 по500; exact segmented noLF | h512-ref-two; отдельный donor. Не тот же TRAIN, что AGC/FBM либо D-T |
| MASK+MOMENT | [MASK:30–36](experimets/cards/E-SFT-RESPONSE-MASK.md:30), [MOMENT:30–36](experimets/cards/E-SFT-MOMENT-CONTINUITY.md:30): те же D-T1/2, joint Prompt+LF+Chosen,2/2qualification,SFTmoments1000 | h512-dt-joint; producerMS01, consumerMC01 |
| GRADB+TS+NHEAD | [GRADB:116–127](experimets/cards/GRADB-LOCALITY-01.md:116), [TS:120](experimets/cards/DPO-TOKENIZER-SNAPSHOT-01.md:120), [NHEAD:30](experimets/cards/N-HEAD-01.md:30): одинаковые Prompt/Chosen и no-separator carrier | h512-dt-segmented; producerTS01. KGB dataset_version отличается: фиксировать собственную metadata/admission, не объявлять все JSONLbytesравными |
| PAD | [PAD:138](experimets/cards/E-DPO-VALID-LENGTH.md:138): длинный chosenPAD2, другие rejected | h512-pad-segmented; не короткийD-T2 |
| ORDER | [ORDER:138–145](experimets/cards/E-DPO-SFT-ORDER.md:138): D-T1/2 segmented + S1/2jointLF,250циклов,4/4 | h512-order-mixed, отдельный |
| AGC+FBM | [AGC:30–36](experimets/cards/GRAD-ACCUM-COPY-01.md:30), [FBM:30–36](experimets/cards/FROZEN-BROADCAST-MAP-01.md:30): единственный f06-008×1000 | h512-postfix-one; producerAGC1, consumerFBM1 |
| RMScoordinate/ROPE/RMSrow/ATTNtranspose | Их строки28: sameH64/seeds/400T±, но rms-coordinate-probe-v1 / rope-training-table-probe-v1 / rms-row-reduce-probe-v1 / attention-transpose-probe-v1 | Четыре family; нельзя silentlyunify streams |
| CS01 | [incoming:91–123](C:/Users/lubao/Documents/xiigen/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-21-learning-delta-26332500/neural/ATTN-CAUSAL-SCORES-01.md:91): frozen-transpose stream, privateSHA literals сCS01 | Отдельная h64-causal-scores |
| SILU | [SILU:115–132](experimets/cards/SILU-SIGMOID-REUSE-01.md:115): frozen-transpose,TotalSteps400,8commonTRAIN/EXAMscores; nativeRNGtype есть вclosure:178 | Отдельная h64-silu; contract33 назначил точный privateRNGidentity и полныйbundle, не притворился что historicalprobe его сохранила |
| SC01/H16 и SEXP/PA01/N63IL | [29:22,44–50](experimets/29-learned-start-and-parent-continuation.md:44) | Существующие H16 donor/accepted semantics сохраняются; SC400Calibrate/Compare не redundantbootstrap, не удалять |
| NEURAL-FROZEN-TRANSPOSE | Сохранённый consumed опыт | Никакой новой preparation/observer/попытки |

«Совместимы» здесь означает точную авторскую композицию будущего общего producer, с native identity/shape/data checks до употребления; не доказанный ABI/load/learning PASS. Карточки сохраняют собственные source pins. OriginSFToptimizer никогда не выдаётся за DPOcontinuation.

## Первичные API и sharedbundle

- [generation-v1.json:7–53](C:/Projects/llm_mvp_core/config/code-documentation/generation-v1.json:7) прочитан полностью: H512/V629/L4/Q8/KV2/FF1408/C2048; rank8/alpha16/dropout0; root/base20261014; policy/reference domains разные. tokenizer.json разобран как JSON (Vocab/Merges), не исполнялся Encode.
- [Trainer:67–93](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:67): native SFT принимает реальные IDs иpositiveweight, bounds по model.ContextLen. Common source/metadata не заменяет точные token IDs.
- [AdamW:188–243](C:/Projects/llm_mvp_core/04.TrainingLoop/Optimizers/AdamW.cs:188): named portable entries, дубликаты/shape invalid отказывают; отсутствующее state не синтезируется. TryImport true не доказывает побитноеравенствоm/v.
- [LlmModel:326–409](C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:326): invalid version/algorithm/hash/step, topology/LoRA mismatch, slots, StreamDomain иseed mismatch отказывают доrestore. Это прямое основание разнести H64families.
- [Randomness/LlmModelRandomState.cs:12–38](C:/Projects/llm_mvp_core/03.TransformerModel/Randomness/LlmModelRandomState.cs:12): переносимый record содержитAdapterKey,StreamDomain,InitializationSeed,DropoutSeed,ConsumedCursor,TopologySha256,LoraConfigurationSha256,RandomStateStep.
- Native authority afterS0 остаётся назначенной в [N61:24](experimets/cards/N61-dpo-reference-reuse.md:24): B0guard→S0load→newrecipient/profile→ownadmission/lease/capability. Новый consumer не переносит чужие lease/capability и не ослабляетB0/base guards.

Contract33 требует weights+keyedAdamW+fullRNG+config/tokenizer+точные source/material bytes+qualification+provenance. ExportExisting ничего не обучает/не оценивает и не восполняет отсутствующие компоненты. Preparation одного семейства не стартует из consumer при missingbundle. Для будущих sourcehosts используется исходнаяA branch собственного fixedclosure; никакой новый trainerexecutor не выбирается.

## Существующие артефакты: адресный снимок, не разрешение продолжать

| Фактический путь | Что прочитано / существует | Ограничение |
|---|---|---|
| C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/study.json | JSONполностью; exacttwo-source-rowsSHA75fa380a51e96083332c9609ae69ce368b04e29011fab4af0c0fc161335202c8, B0weights/manifest/config identity, planned1000+4 | План/ownerreceipt, не completedqualification |
| .../N61-REF-CACHE/preparation.log | Один ограниченный read-snapshot: последнее прочитанное update750/1000, loss.006628; sourceписалlogпараллельно | Не finalreceipt и не доказательство дальнейшего результата. В адресном каталоге не было commoncompletebundle. Не следил за процессом и не определял dotnetвладельца |
| C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/trained-weights.bin; trained-optimizer.bin | Existence/length43500/1112. restore-minimal-set.json структурно parsed; cfgH16,step1000,restoredpanelidentical; weightstatehashbefore/afterload одинаковый | Known3/40tokens/full0/2 — partial, не universalqualifiedskill. RawRNGsnapshot рядом не найден; binarydeserialization/hash этого прохода не выполнялись |
| C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63R-PROBE/results/wa-trained.bin; wa-trained-optimizer.bin | Existence/length107244/7384; n63r-probe.json структурноparsed | H32, другойTRAIN/shape. Не H512/H64; нет нового semanticreview всех698KBraw |
| C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/results/authority-acquired.bin | Existence47899519bytes. n63c-probe.json parsed; acquisition600updates,final22/23,teacherForcedResponseExact=false | Rawverdict “ACQUIRED” не полный23/23. Другойlesson/framing; completecommonSFToptimizer/RNGне найден. results-02authorityexists не означает completequalifiedbundle |
| C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/probe-result.json | JSONparsed; ProgramSeed/stream и отсутствиеcheckpointwriter адреснопрочитаны; деревоtextreceipts | Historical1/2 — recipeevidence, не сохранённый fullbundle. Не запускал/непересобирал |
| PA01, MS01,MC01,PD01,OR01,TS01,NH01,AGC1,FBM1 и перечисленныеH64futurehosts | Адресная inventoryassignedhostnames | Не объявлены материализованными/готовыми; PA01futureacceptedpointerнеcanonicalpublication |

RESTORE source [Program:78–113](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs:78) показываетseed20261020/streamrestore-minimal-set, SaveCheckpoint/portable import. N63R source [Program:92–95](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63R-PROBE/Program.cs:92) сохраняет WAweights+optimizer. LEARNSANITY [Program:51–54](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/Program.cs:51) используетдругойH64stream. Это sourcefacts, не независимая custodyаттестация старых binaries.

## Реальный охват и оставшиеся пределы

Полностью прочитан55строчныйcontract29; attachment7043 — весь текст выбраннымичастями. Protocol24/common-readout clauses, protocol27loader/commonclauses, исторический25donor — TARGETED_READ, не новые archaeology/bytesанализы. 15-commonrecipe набор прочитан по preparation/data/state/budget sections, затем SILUиCS01 адресно для familyразделения; не полное semanticapproval всех карточек. Предыдущий14308evidenceиспользован как карта, не новый измерительный receipt.

JSON сырьё parsed структурно с адресным чтением перечисленныхзначений; binarypayloadsтолько existence/length, не load/hash/ABI. Filesystemoutputs были bounded; обрезанные большие tooloutputs не считаются прочитанными полностью. В этом пакете нет новых empiricalуспехов, whole-corpusnoveltyreview или зависимости от processname.

Открыто: будущая реализация exactPrepare/Export, фактическая identity/readback/qualification и runtimeсовместимость sharedbundle. Это не поиск исполнителю: paths/data/budget/producer/consumers уже назначены; отказ сохранит конкретное поле/guard иOPEN, не другойseed/root/автоподготовку. ТекущаяLCпопытка исключена. Толькоparentприменяет mappingblocks в карточки/index. Новых runtimeauthority этим документом не добавлено.

Финальное документное уточнение: exact private RNG literals/алгоритм заданы для всех шести H64families в33/JSON. Existing export требует четырёх настоящих native-state/qualification источников, но не притворяется, что старый common уже содержит11sharedfiles: описательные metadata разрешено вывести из fixed sources без Train/Forward/Generate с provenance derived_for_export; отсутствующий RNG/optimizer/учебный результат не дорисовывается. Read-snapshotLC относится к21.10.2026: последняя прочитанная строка журнала [08:21:26]update750; дальнейшее состояние не отслеживалось, это не текущий live status.

Адресный повторный readback MOMENT: карточка36/42/138–142 сохраняет A reset/B carry перед вторым уроком,1001→A1/B1002. В33 и JSON это явно сохранено: shared import только перед первым уроком; повторный import перед вторым запрещён, иначе он отменил бы изучаемый фактор. Никаких правок самого MOMENTфайла этим workerне сделано.

