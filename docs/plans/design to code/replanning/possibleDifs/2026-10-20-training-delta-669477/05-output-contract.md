# Пути, команды и единственный владелец попытки

**Текущая фаза — только документирование. NEXT: N63-INGRESS-LENGTH.** Здесь 33 будущих назначения: 29 из core6e, 2 из соседнего planning-пакета и новые VT/RMS. Первые 7 completed не получают нового output-root, wrapper или TRAIN-панели. [Машинная карта](output-path-map.json) соответствует [реестру](04-execution-registry.md). Наличие команды — рецепт, а не утверждение о существующем или проверенном executable.

## 1. Где сохраняется каждый будущий результат

R означает единственный result-root. Относительные пути raw/arms/checkpoint остаются из точной карточки; report и кандидат ниже находятся в R. Полные host roots и source URL включены в JSON. Все пути буквальные; искать иной checkout или автоматически выбирать другой attempt не нужно и не разрешено.

| № | ID | Единственный R | Русский отчёт / кандидат |
|---:|---|---|---|
| 08 | [N63-INGRESS-LENGTH](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/N63-ingress-length.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63I/results` | `report.md` / `candidate.md` |
| 09 | [N63-AR-CAP](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/N63-replay-capacity.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63R/results` | `report.md` / `candidate.md` |
| 10 | [N63-CORRECTION-COLLISION](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C/results` | `report.md` / `candidate.md` |
| 11 | [WIKI-TAXONOMY-01](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-TAXONOMY-01.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WT01/results` | `report.md` / `candidate.md` |
| 12 | [WIKI-MARGIN-01](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-MARGIN-01.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WM01/results` | `report.md` / `candidate.md` |
| 13 | [N60-E1](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/N60-e1-verifier-admission.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results` | `report.md` / `candidate.md` |
| 14 | [LC-REF-CACHE](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/N61-dpo-reference-reuse.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N61-REF-CACHE/results` | `report.md` / `candidate.md` |
| 15 | [RETENTION-M1-01](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/N61-mode1-retention-and-identity.md#L6) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01` | `report.md` / `candidate.md` |
| 16 | [T-TRAIN-CONTEXT-01](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/N62-training-context.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results` | `report.md` / `candidate.md` |
| 17 | [E-SFT-RESPONSE-MASK](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/E-SFT-RESPONSE-MASK.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MS01/results/attempt-0001` | `report.md` / `candidate.md` |
| 18 | [E-SFT-MOMENT-CONTINUITY](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/E-SFT-MOMENT-CONTINUITY.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MC01/results/attempt-0001` | `report.md` / `candidate.md` |
| 19 | [E-DPO-VALID-LENGTH](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/E-DPO-VALID-LENGTH.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PD01/results/attempt-0001` | `report.md` / `candidate.md` |
| 20 | [E-DPO-SFT-ORDER](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/E-DPO-SFT-ORDER.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/OR01/results/attempt-0001` | `report.md` / `candidate.md` |
| 21 | [KH-COST-01](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/KH-COST-01.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KHC/results` | `report.md` / `candidate.md` |
| 22 | [KH-EXT-01](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/KH-EXT-01.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KHE/results` | `report.md` / `candidate.md` |
| 23 | [GRADB-LOCALITY-01](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/GRADB-LOCALITY-01.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KGB/results` | `report.md` / `candidate.md` |
| 24 | [RMS-COORD-ALLOC-01](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/RMS-COORD-ALLOC-01.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001/results` | `report.md` / `candidate.md` |
| 25 | [DPO-TOKENIZER-SNAPSHOT-01](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/DPO-TOKENIZER-SNAPSHOT-01.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/results/attempt-0001` | `report.md` / `candidate.md` |
| 26 | [BPE-WEIGHTED-CHUNKS-01](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/BPE-WEIGHTED-CHUNKS-01.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BW01/results/attempt-0001` | `report.md` / `candidate.md` |
| 27 | [WIKI-FEATURE-KEYS-01](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-FEATURE-KEYS-01.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/results/attempt-001` | `report.ru.md` / `candidate.md` |
| 28 | [WIKI-FEATURES-ONCE-01](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-FEATURES-ONCE-01.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/results/attempt-001` | `report.ru.md` / `candidate.md` |
| 29 | [N-HEAD-01](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/N-HEAD-01.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NH01/results/attempt-0001` | `report.md` / `candidate.md` |
| 30 | [METHOD-KNN-TARGET-SCOPE-01](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/METHOD-KNN-TARGET-SCOPE-01.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/results/attempt-0001` | `report.md` / `candidate.md` |
| 31 | [ROPE-TRAIN-TABLE-01](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/ROPE-TRAIN-TABLE-01.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RPE01/a001/results` | `report.md` / `candidate.md` |
| 32 | [AST-SIB-CACHE-01](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/AST-SIB-CACHE-01.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01/results/attempt-0001` | `report.md` / `candidate-fix.md` |
| 33 | [NG01](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/NG01-null-guard-label.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/r/attempt-0001` | `report.md` / `candidate.md` |
| 34 | [BPE-REALIZED-MERGE-COUNT-01](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/BPE-REALIZED-MERGE-COUNT-01.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/r/0001` | `report.md` / `candidate.md` |
| 35 | [GRAD-ACCUM-COPY-01](https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/GRAD-ACCUM-COPY-01.md) | `C:/xg/AGC1/results` | `report.md` / `candidate.md` |
| 36 | [FROZEN-BROADCAST-MAP-01](https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/FROZEN-BROADCAST-MAP-01.md) | `C:/xg/FBM1/results` | `report.md` / `candidate.md` |
| 37 | [VT-APOSTROPHE-01](cards/VT-APOSTROPHE-01.md) | `C:/xexp/QVA01/a01` | `report.md` / `candidate.md` |
| 38 | [RMS-ROW-REDUCE-01](cards/RMS-ROW-REDUCE-01.md) | `C:/xg/RD01/a001/results` | `report.md` / `candidate.md` |
| 39 | [IDENTITY-M1-01](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/N61-mode1-retention-and-identity.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N61-MODE1/results/IDENTITY-M1-01` | `report.md` / `candidate.md` |
| 40 | [KN-ROUTER-LEARNED-KEY-01](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/KN-ROUTER-LEARNED-KEY-01.md#L3) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KR01/results` | `report.md` / `candidate.md` |

## 2. Какие прежние назначения отозваны

На core6e назначения оставшихся 29 inherited roots и entry совпадают с ранее опубликованными core827 headers. Изменения body paths синхронизируют эти headers. Для LF-03 будущий wrapper и TRAIN-панель отозваны: результат уже завершён. [Последняя квитанция](evidence/core6e-output-map.json).

Future R2/E-OPT/LF-03 отозваны, потому что уже существуют completed результаты. Исходные папки сохраняются. Core827 также явно заменил три прежних planning назначения; это не разрешение скрыто запустить второй раз уже начатую legacy попытку.

| ID | Старое future назначение | Действующий R |
|---|---|---|
| AST-SIB-CACHE-01 | `<Planning>/llm mvp/design to code session/replanning/possibleDifs/runs/AST-SIB-CACHE-01/20261020-01` | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01/results/attempt-0001` |
| NG01 | `C:/xg/NG01/r/attempt-0001` | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/r/attempt-0001` |
| BPE-REALIZED-MERGE-COUNT-01 | `C:/xg/br01/r/0001` | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/r/0001` |

ROPE и NG теперь используют `candidate.md`, AST — `candidate-fix.md`; WIKI-FEATURE-KEYS/ONCE сохраняют `report.ru.md`. Эти различия намеренны. Старые файлы не переименовываются, не перемещаются и не удаляются. Полное происхождение смены путей — [09](09-core-827-delta.md).

## 3. Единственный вход каждого опыта

Сначала материализовать только назначенный isolated host и source copies по карточке, затем выполнить её одну сборку/цепочку. Ниже перечислен runtime entry или одна упорядоченная цепочка; это не команда запускать их сейчас. Wrapper уже владеет внутренними build/prepare/A/B/readout/summary, их не вызывают дополнительно. Если указан прямой DLL entry, его единственная предшествующая сборка и файлы заданы в карточке; новый wrapper не придумывать.

### 08. N63-INGRESS-LENGTH

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/N63-ingress-length.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63I`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63I/Run-With-Learning-Signal.ps1
```

### 09. N63-AR-CAP

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/N63-replay-capacity.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63R`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63R/Run-With-Learning-Signal.ps1
```

### 10. N63-CORRECTION-COLLISION

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C/Run-With-Learning-Signal.ps1
```

### 11. WIKI-TAXONOMY-01

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-TAXONOMY-01.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WT01`.

```powershell
powershell -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WT01/Run.ps1
```

### 12. WIKI-MARGIN-01

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-MARGIN-01.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WM01`.

```powershell
powershell -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WM01/Run.ps1
```

### 13. N60-E1

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/N60-e1-verifier-admission.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION`.

```powershell
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/bin/Release/net9.0/N60E1VerifierAdmission.dll --arm A --output C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results/A
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/bin/Release/net9.0/N60E1VerifierAdmission.dll --arm B --output C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results/B
```

### 14. LC-REF-CACHE

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/N61-dpo-reference-reuse.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N61-REF-CACHE`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N61-REF-CACHE/Run-With-Learning-Signal.ps1
```

### 15. RETENTION-M1-01

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/N61-mode1-retention-and-identity.md#L6); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N61-MODE1`.

```powershell
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N61-MODE1/bin/RETENTION/A/N61Mode1Runner.dll --contrast RETENTION --arm A --output C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/A
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N61-MODE1/bin/RETENTION/B/N61Mode1Runner.dll --contrast RETENTION --arm B --output C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/B
```

### 16. T-TRAIN-CONTEXT-01

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/N62-training-context.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01`.

```powershell
powershell.exe -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/Prepare.ps1
powershell.exe -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/Run.ps1
```

### 17. E-SFT-RESPONSE-MASK

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/E-SFT-RESPONSE-MASK.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MS01`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MS01/Run-With-Learning-Signal.ps1
```

### 18. E-SFT-MOMENT-CONTINUITY

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/E-SFT-MOMENT-CONTINUITY.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MC01`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MC01/Run-With-Learning-Signal.ps1
```

### 19. E-DPO-VALID-LENGTH

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/E-DPO-VALID-LENGTH.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PD01`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PD01/Run-With-Learning-Signal.ps1
```

### 20. E-DPO-SFT-ORDER

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/E-DPO-SFT-ORDER.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/OR01`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/OR01/Run-With-Learning-Signal.ps1
```

### 21. KH-COST-01

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/KH-COST-01.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KHC`.

```powershell
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KHC/bin/Release/net9.0/KhCostRunner.dll --arm A C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KHC/results
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KHC/bin/Release/net9.0/KhCostRunner.dll --arm B C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KHC/results
```

### 22. KH-EXT-01

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/KH-EXT-01.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KHE`.

```powershell
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KHE/bin/Release/net9.0/KhExtRunner.dll --arm A C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KHE/results
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KHE/bin/Release/net9.0/KhExtRunner.dll --arm B C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KHE/results
```

### 23. GRADB-LOCALITY-01

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/GRADB-LOCALITY-01.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KGB`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KGB/Run-With-Learning-Signal.ps1
```

### 24. RMS-COORD-ALLOC-01

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/RMS-COORD-ALLOC-01.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001/Run-With-Learning-Signal.ps1
```

Особенность из source contract: Run-With-Learning-Signal claims host then invokes run.py once; no external pre-build or precreation of results/bin/obj; run.py remains sole build/result/A-B owner.

### 25. DPO-TOKENIZER-SNAPSHOT-01

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/DPO-TOKENIZER-SNAPSHOT-01.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/Run-With-Learning-Signal.ps1
```

### 26. BPE-WEIGHTED-CHUNKS-01

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/BPE-WEIGHTED-CHUNKS-01.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BW01`.

```powershell
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BW01/bin/A/Release/net9.0/BpeWeightedRunner.dll run-pair --peer C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BW01/bin/B/Release/net9.0/BpeWeightedRunner.dll --fixture C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BW01/in/fixture.json --attempt 0001
```

### 27. WIKI-FEATURE-KEYS-01

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-FEATURE-KEYS-01.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01`.

```powershell
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/bin/WikiProbe.dll --arm A --output C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/results/attempt-001/A
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/bin/WikiProbe.dll --arm B --output C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/results/attempt-001/B
```

### 28. WIKI-FEATURES-ONCE-01

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-FEATURES-ONCE-01.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01`.

```powershell
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/bin/WikiProbe.dll --arm A --output C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/results/attempt-001/A
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/bin/WikiProbe.dll --arm B --output C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/results/attempt-001/B
```

### 29. N-HEAD-01

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/N-HEAD-01.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NH01`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NH01/Run-With-Learning-Signal.ps1
```

### 30. METHOD-KNN-TARGET-SCOPE-01

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/METHOD-KNN-TARGET-SCOPE-01.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01`.

```powershell
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/bin/Release/net9.0/MkScopeRunner.dll --prepare C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/results/attempt-0001
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/bin/Release/net9.0/MkScopeRunner.dll --arm A C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/results/attempt-0001/A
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/bin/Release/net9.0/MkScopeRunner.dll --arm B C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/results/attempt-0001/B
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/bin/Release/net9.0/MkScopeRunner.dll --summarize C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/results/attempt-0001
```

Особенность из source contract: Keep the assigned prepare -> A -> B -> summarize sequence. Do not invent a second Run-Assigned wrapper or repeat prepare in observer.

### 31. ROPE-TRAIN-TABLE-01

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/ROPE-TRAIN-TABLE-01.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RPE01/a001`.

```powershell
Set-Location 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RPE01/a001'; & 'C:/Python314/python.exe' ./run.py
```

### 32. AST-SIB-CACHE-01

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/AST-SIB-CACHE-01.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01/Run-Assigned.ps1
```

Особенность из source contract: SourceCore=C:/Projects/llm_mvp_core read-only; Host=Core=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01; Native=Host/Native/A.cs and B.cs; Planning=C:/Users/lubao/Documents/xiigen/llm mvp/design to code session read-only. Run-Assigned claims host before prepare; prepare alone claims result; no second prepare/owner; 4 train arms nA, nB, wB, wA; old Planning output and C:/x/ast01 retired.

### 33. NG01

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/NG01-null-guard-label.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01`.

```powershell
pwsh -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/Run.ps1
```

### 34. BPE-REALIZED-MERGE-COUNT-01

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/BPE-REALIZED-MERGE-COUNT-01.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/Run-Assigned.ps1
```

### 35. GRAD-ACCUM-COPY-01

[Карточка и встроенный промпт](https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/GRAD-ACCUM-COPY-01.md); [общий договор](https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/TENSOR-LEARNING-EXECUTION-CONTRACT.md); host `C:/xg/AGC1/host`; claim `C:/xg/AGC1/host/execution.claim`. Исходный пакет сохранён, runtime здесь не выполнялся.

```powershell
pwsh -NoProfile -File 'C:/xg/AGC1/host/Run.ps1'
```

### 36. FROZEN-BROADCAST-MAP-01

[Карточка и встроенный промпт](https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/FROZEN-BROADCAST-MAP-01.md); [общий договор](https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/TENSOR-LEARNING-EXECUTION-CONTRACT.md); host `C:/xg/FBM1/host`; claim `C:/xg/FBM1/host/execution.claim`. Исходный пакет сохранён, runtime здесь не выполнялся.

```powershell
pwsh -NoProfile -File 'C:/xg/FBM1/host/Run.ps1'
```

### 37. VT-APOSTROPHE-01

[Точная карточка](cards/VT-APOSTROPHE-01.md); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/QVA01`.

```powershell
pwsh -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/QVA01/launch.ps1
```

### 38. RMS-ROW-REDUCE-01

[Точная карточка](cards/RMS-ROW-REDUCE-01.md); host `C:/xg/RD01/a001`.

```powershell
Set-Location 'C:/xg/RD01/a001'
python ./run.py
```

### 39. IDENTITY-M1-01

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/N61-mode1-retention-and-identity.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N61-MODE1`.

```powershell
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N61-MODE1/bin/IDENTITY/A/N61Mode1Runner.dll --contrast IDENTITY --arm A --output C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N61-MODE1/results/IDENTITY-M1-01/A
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N61-MODE1/bin/IDENTITY/B/N61Mode1Runner.dll --contrast IDENTITY --arm B --output C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N61-MODE1/results/IDENTITY-M1-01/B
```

### 40. KN-ROUTER-LEARNED-KEY-01

[Точная карточка](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/KN-ROUTER-LEARNED-KEY-01.md#L3); host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KR01`.

```powershell
powershell -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KR01/Run.ps1
```

## 4. Lifecycle без самоблокировки и повторного обучения

**После каждого subprocess проверяется exit code и timeout. Ненулевой код или timeout останавливает оставшуюся цепочку: после неуспешного A плечо B не запускается.** Это правило относится и к приведённым прямым DLL-командам; их нельзя безусловно исполнять все подряд. Сам STOP имеет приоритет даже перед сохранением status.

Входные checkpoint/tokenizer/source identities остаются read-only и не перенаправляются. Если старый CLI/bridge/observer содержит protected output, заменить его только в будущей isolated copy на единственный R симметрично для A/B; callbacks и guards получают тот же путь. Protected `docs/plans/replanning-possible-diffs`, включая `required-changes.md`, в текущем поручении изменять нельзя: адресный кандидат сохраняется только в R.

Host-owned `execution.claim`, `study.json`, `train.jsonl`, source/dependency manifests и build provenance остаются там, где их требует recipe. Не создавать results преждевременно. Единственный execution/prepare owner резервирует R до native Train; observer не делает второй prepare. При существующем claim/result/partial/неизвестном процессе сохранить состояние и остановить эту попытку с техническим статусом; нельзя обходить guard новой папкой, ID или номером attempt.

AST current827: Host=Core=AST01, Native=Host/Native/A.cs и B.cs, SourceCore и Planning read-only. Только `Run-Assigned.ps1` выполняет prepare/build/nA/nB/wB/wA/compare; prepare один раз резервирует Out. Старый поиск sibling Planning/WorkCore C:/x/ast01 и запись Planning/runs отозваны. Для METHOD-KNN сохраняется prepare→A→B→summarize без второго wrapper. Для RMS один run.py создаёт results и владеет сборкой; внешний подготовитель не создаёт results/bin/obj заранее.

Ни Windows build, ни path-length проверки этих старых hosts здесь не исполнялись. Длинный путь/отсутствующий SDK/сбой build до Train — технический блокер, не negative accuracy и не повод выбрать другую гипотезу. Actual counters брать из durable receipt; при его отсутствии null/«не измерено». Не учиться ещё раз ради получения counter или отчёта.

Обычное завершение: русский report с фактическими A0/A1/B0/B1 и затратами, рядом regressions и один candidate с original file/method/предлагаемым переносом. Любой полезный измеренный эффект — успех по своей оси и теме. Модель исполнителя — default текущей сессии, skills и данные заданы карточкой. Полного product fix/CI здесь нет.

**STOP/NEXT немедленный:** прекратить оформление прежнего опыта, hash/review/retry/Git сразу после команды перейти; полный STOP прекращает всё, кроме необходимого прекращения собственного активного процесса. Даже запись status не является исключением из прямого приказа остановиться. Старые результаты не удалять.
