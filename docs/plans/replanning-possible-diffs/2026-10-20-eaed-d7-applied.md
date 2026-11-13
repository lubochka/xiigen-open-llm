# Карточки после пакетов eaed и d7

**Исторический срез40 ID.** Текущий [срез0ea](2026-10-20-0ea-applied.md) добавляет ATTN/JSON и обновляет очередь42; ниже сохранено состояние до него.

20 октября 2026. Правки применены только в C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs. Это исследование и документация: hosts, исходники экспериментальных проектов, fixture, salt, weights и runtime state не создавались. Сборки, тесты, Train и inference не запускались.

**Следующий незавершённый — [№08 N63-INGRESS-LENGTH](experimets/cards/N63-ingress-length.md).** [Индекс](experimets/cards/00-execution-index.md) содержит 40 уникальных ID: 7 saved и 33 будущих. [Единый контракт](experimets/26-output-and-entry-contract.md) и [машинная версия](2026-10-20-card-reconciliation/d7-execution-contract.json) назначают каждому будущему ID один R, полный путь карточки, entry и NEXT. Модель исполнителя — текущая модель сессии по умолчанию без override; Student задаётся отдельно.

## Сначала обучение

Четыре новых фактора перенесены в действующие карточки с сохранением их данных и бюджетов:

| ID / очередь | Участок настоящего обучения | Единственное размещение под core/experiments-v2/RP01-MICRO |
|---|---|---|
| [GRAD-ACCUM-COPY-01, №35](experimets/cards/GRAD-ACCUM-COPY-01.md) | Первый AccumulateGrad: убрать внешнюю копию, сохранив TensorStorage copy | AGC1/src, AGC1/host, AGC1/results; host/Run.ps1 |
| [FROZEN-BROADCAST-MAP-01, №36](experimets/cards/FROZEN-BROADCAST-MAP-01.md) | Не создавать backward index map при обоих RequiresGrad=false в DPO forward | FBM1/src, FBM1/host, FBM1/results; host/Run.ps1 |
| [VT-APOSTROPHE-01, №37](experimets/cards/VT-APOSTROPHE-01.md) | Builder→native Train→required-term memory→Verify: сохранить brother's | QVA01/src, QVA01 host, QVA01/results/a01; launch.ps1 |
| [RMS-ROW-REDUCE-01, №38](experimets/cards/RMS-ROW-REDUCE-01.md) | SumAlongAxis последней оси внутри SFT/LoRA, прежний float order | RD01/a001; results внутри него; Python314 run.py |

GRAD и FROZEN имеют отдельные пары A/B: по одному native DPO update на arm и четыре full-response диагностических forwards на ID. Это не saved LF first-token панель. [Их самостоятельный договор](experimets/27-tensor-learning-execution-contract.md) содержит фиксированную closure из114 source files, четыре local libraries, ABI boundary, loader/pins, точные request fields и одного study owner. Live N62 runtime отозван и не служит инструкцией подготовки.

VT сохраняет две TRAIN пары,4 Unit updates и32 Verify. Q7/Q8 с перефразировкой/U+2019 остаются допустимыми: возможное снижение false accept показывается вместе с ростом false reject. Прогноз6/8→6/8 не записан как измерение; полный Translation Flow не заявлен. RMS сохраняет8 SFT updates и16 readouts; source и kernel отличаются от RMS-COORD/FROZEN. Измеренные выигрыши новых четырёх факторов пока отсутствуют.

В [P78](experimets/cards/P78-loss-plateau.md), [P89](experimets/cards/P89-density-stop.md), [P119](experimets/cards/P119-learning-dynamics.md) добавлен самостоятельный gap E-METRIC-PHASE: caller отдаёт monitor loss, вычисленный до optimizer. Window1→5 и weighted→normalized — другие факторы. Для phase-only опыта модель/данные/host ещё не назначены автором; статус NEEDS_INPUT_FOR_RECIPE, без выдуманного запуска.

## Конфликты и текущие границы

Все новые C:/xg и C:/xexp output назначения заменены внутренними. Старый claim/partial по прежнему пути сохраняется и не разрешает повтор после переноса. До build — ранний host claim; до learner/readout/Train — arm claim; R резервирует ровно один штатный owner. Timeout/unknown outcome не приводит к a002 или rerun. STOP/NEXT немедленны, даже когда report не дописан.

FROZEN→VT→RMS→IDENTITY39→KN40 согласованы в карточках, промптах и реестрах. SKNN остаётся alias AST. Старые source snapshots36/38 и NEXT LF помечены историческими. Семь сохранённых опытов не получают новой панели и не повторяются. EOPT/LF raw metadata/custody gaps сохранены в прежних errata; R1/R2 useful gains и регрессии остаются кандидатами.

В [P54](experimets/cards/P54-phrase-composition.md) уточнено различие guard перед всеми routes и SourceSpanFrames только перед phrase return. Семь P54 recipes остаются NEEDS_INPUT с конкретными отсутствующими snapshots/contracts. Успешные исторические Unit/kNN результаты и ненулевые RP01 margins не объявлены доказательством собственной правильной генерации или единственной причины неудач.

## Что прочитано и что проверялось

Поступления: eaed61 файлов плюс pointer; d7 ещё58 файлов плюс общий README. Оба publication summaries учтены, причём ebaf повторяет старый53cb snapshot с устаревшим NEXT. Полное файловое/JSON чтение отделено от адресного смыслового и первичного разбора. Все14 вложений переписки входят в накопительный реестр. Исторические чужие reviews, сотни ledger rows и Source manifest не считаются нашими независимыми reviews или запусками.

[Evidence помощника /root/rp01_primary_evidence](2026-10-20-eaed-evidence.md) фиксирует охват, primary symbols, source pins и границы вывода. Статический readback очереди, ссылок, defaults/STOP, output contracts и новой source closure сохраняется в card-reconciliation. Это не компиляция и не подтверждение runtime ABI. Новых численных экспериментальных результатов в этой сессии нет.
