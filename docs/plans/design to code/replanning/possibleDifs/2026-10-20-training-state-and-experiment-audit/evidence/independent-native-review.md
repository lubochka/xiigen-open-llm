# Независимое review основного пакета и resource scaffolds

Reviewer: branches_units. Прочитаны root README и документы01–05, готовые C#/Python/csproj scaffolds `linear-transpose` и `bpe-copy`, их `one-factor.diff`, связанные native Trainer/LoRA/Transpose/BPE блоки. Обучение, build, inference и CI не выполнялись. .NET SDK отсутствует. Это source/contract review, а не runtime подтверждение.

## Замечания, переданные авторам

| ID | Замечание и конкретное исправление | Статус при записи review |
|---|---|---|
| XR-01 | В `compare.py` resource hosts `valid` не включал initial identity, BPE не проверял одинаковый EXAM; `successes_by_metric` заполнялся даже при `actual_learning_observed=false`. Это позволяло выдать цену неудавшегося/другого обучения за причинный успех. Нужна отдельная диагностическая ветвь при недостигнутом writer/различных initial inputs, без скрытия raw deltas. | Автор исправил; повторно прочитаны оба `compare.py`: `INVALID_CAUSAL_COMPARISON_DIAGNOSTIC_ONLY`, пустой список causal successes, raw beneficial deltas сохранены. Final state/output mismatch остаётся отдельным результатом, а не поводом скрывать честный ресурсный выигрыш. |
| XR-02 | Оба `Program.cs` читали второй `Process.TotalProcessorTime` до `Refresh`. При возможном cached ProcessInfo CPU-поле могло быть недостоверным. Вызвать `Refresh` перед вторым чтением; wall/alloc boundaries оставить вокруг Train. | Автор исправил. В materialized linear Program L62–63, BPE L39–40 порядок проверен. Это hardening измерителя, не доказанный текущий runtime ноль. |
| XR-03 | Root `[wiki-enum]` ссылался на L462–484, что не включает весь17-элементный enum. | Root уведомлён: точный диапазон pinned `ProjectWikiDomainModels.cs#L473-L492`. |
| XR-04 | `linear-transpose` comparator должен включать `frozen_unchanged_each` в admission: immutable-base — предпосылка корректности повторного transpose. Если она нарушилась, это не валидный опыт заявленного frozen cache. | Автор добавил `frozen_unchanged_each` в admission; повторно прочитано. Native caller также исключает frozen parameters из optimizer. |
| XR-05 | В корне каждого scaffold остался лишний `Runner.csproj` с `ArtifactRoot=../../../../experiments-v2/...`; такой relative path верен только для настоящего вложенного host под `35.ExperimentsV2/Experiments/Rp01Micro/ID`. Два разных места с одинаковым именем создают ненужный command trap. | Автор сообщил удаление, но повторный `rg --files` пока всё ещё показывает4 Runner.csproj; передано повторное замечание, требуется финальная сверка packaging. Рабочий `run.py` выбирает правильные вложенные host. Старые эксперименты не затрагиваются. |

## Что подтверждено чтением source

### NEURAL-FROZEN-TRANSPOSE-01

- `Program` создаёт native `LlmModel`, применяет `LoraApplicator.ApplySeeded` к q/v, вызывает четыре настоящих `Trainer.TrainSupervisedNextTokenStep` на фиксированных двух строках, а native метод вызывает `BackwardLoss` и `AdamW.Step` только для `RequiresGrad` parameters. Это учебный опыт; не forward-only benchmark и не симуляция Adam в Python.
- Перед A0/B0 cache flag выключен. B включает его только после начального EXAM; lazy fill входит в замер TRAIN. Бесплатного прогрева B нет.
- Изменён один native файл `Linear.cs`. Cache guard — `ExperimentReuseFrozenTranspose && !Weight.Value.RequiresGrad`. LoRA A/B обрабатываются актуальным native путём; они не кэшируются. `TensorOps.Transpose` создаёт grad graph только при `x.RequiresGrad`, поэтому frozen cached tensor не переносит graph обучаемого weight между шагами.
- Cache lifetime ограничен одной созданной и не изменяемой моделью; restore, разморозка базы, replacement tensors, concurrent training, cross-model reuse не входят в опыт. Не утверждается production cache invalidation для этих случаев.
- Бюджет A+B:8 native SFT updates,0 DPO steps,8 EXAM forwards (2 запроса × before/after ×2 arms). Full-sequence next-token objective явно отмечен; изменение исключительно answer-mask сюда не добавлено.
- До/после сохраняются target NLL, exact sign, logits, parameter hashes, optimizer moments, losses и costs. Одного короткого измерения недостаточно для claims устойчивого ускорения. Пик рабочего набора относится ко всему процессу до окончания TRAIN, а retained-cache payload сообщается отдельно.
- Формулировка root «ресурсная ось», отделённая от RP01 accuracy/DPO, корректна. Сохранённый final identity проверяет вычислительную эквивалентность; если он не совпадает, это вывод о семантическом побочном эффекте, который должен сопровождать измеренную цену.

### BPE-NO-MATCH-COPY-01

- Измеряется native `BpeTokenizer.Train(corpus,320)`, изменяющий merges/vocabulary, затем native Encode/Decode. Первое `Train("",256)` создаёт исходный byte vocabulary и не выдаётся за отдельное приобретение. Для каждого A/B/domain одна непустая учебная трансформация.
- B меняет только no-match allocation в `MergeChunk`: при первом совпадении копирует точный prefix, затем выполняет прежнее left-to-right nonoverlapping merge; без совпадения возвращает исходный список. Native callers переустанавливают chunk references и не мутируют возвращённый no-match input in place. Tie selection, merge order/ranks/vocabulary не меняются.
- Два домена фиксированы: `code-doc`, `en-ru`;5 raw TRAIN строк ×8 включений в один corpus,4 EXAM. A+B по двум доменам:4 непустых native Train,4 пустых initialization Train,32 Encode/Decode readouts. Восемь повторов строк — состав одного corpus, не8 автоматических экспериментальных запусков.
- Это экономия обучения tokenizer с будущим dependency на `KnnLogicalUnit.Train`; само качество kNN и neural не измеряется, что явно сказано. Roundtrip/exact tokens — контроль сохранения преобразования, а не отдельная уловка выдавать serving за обучение.

### Commands и сборка

- `run.py` использует конкретный nested csproj, не solution/dispatcher/42.ProjectWikiDomain; проверяет hashes materialized source и Program; старый results directory не перезаписывает; число jobs фиксировано. При timeout не дополняет прогон новыми попытками.
- `prepare.py` сначала проверяет pinned Git blobs всех входов, затем пишет только новый planning artifact; core используется read-only. Повторная подготовка поверх существующей копии останавливается.
- Полный compile runtime остаётся `NOT_BUILT`, не `PASS`: source closure/AST/manifest проверки не доказывают сборку. В root документах это сохранено.

## Root claims и границы

- EN→RU результаты0/24→19/24, ingress11/24→19/24 и cold restore19/24 разделены; они не склеены в один путь0→11→19. Проверено по текущему P125 report:144 Flow calls,16 уроков B/C, target-swap control13/24, три false accepts.
- Memory/classical/LoRA/DPO разграничены. E09 unequal denominator и E25 POS-vs-translation ограничение прямо указаны; historical replay не назван neural resume.
- Новые Wiki findings описаны как локальная модель/reader, свежий Train не приравнивается к миграции старого snapshot; кандидат taxonomy сохраняет15 координат. В main01 полезно сохранить explicit static-orchestrator limitation, уже подробно данную в Wiki карточке, чтобы краткий главный текст не воспринимался как доказанный product adoption.
- Девять правил пользователя отражены корректно, включая успех по отдельной полезной метрике при регрессиях другой оси, отсутствие production изменений и запрет защищённой папки.
- Root сообщил о свежем docs-only delta core7c5718 и planning64aa3c. Known N61/reference-coverage изменения сейчас пересчитывает cards_audit; ранее прочитанный вывод «четыре inherited gaps» не фиксируется этим review как окончательный баг. Перед публикацией действует финальный source ledger/reconciliation.

## Итог review

Writer/state/reader wiring и single-factor смысл обеих resource гипотез подтверждены source review. Выявленные causal admission/CPU наблюдения исправлены автором, immutable-base premise исправлен и повторно проверен; packaging duplicate ещё требует финальной сверки, точность enum anchor передана root. Новых runtime результатов этот review не создаёт; положительный эффект и пригодность production переноса остаются вопросом будущего ограниченного опыта.
