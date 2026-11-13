---
title: "348. Algorithm 9 и рисунки роста: рабочая схема с ещё не заданными гарантиями"
source_pdf_pages: [779, 780]
book_pages: [743, 744]
batch: "016"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 348. Algorithm 9 и рисунки роста: рабочая схема с ещё не заданными гарантиями

## Вопрос

Что уже задано в controller, а что нельзя считать подтверждённым по рисункам 19.8–19.9?

**Algorithm 9 задаёт понятный цикл наблюдения, роста, удаления и замораживания. Его рисунки прямо названы синтетическими иллюстрациями и не подтверждают ни ускорение обучения, ни точную непрерывность рождения.**

Источник: [Algorithm 9](../source/0777-0779-ch19-incremental-birth-monitoring-and-controller.md#algorithm-9), [§19.6.6 и Figures 19.8–19.9](../source/0780-ch19-didactic-growth-and-geometry-illustrations.md#section-19-6-6).

## Определённая часть схемы

Сначала выполняется gradient update, затем warm-up недавно созданных heads. На monitoring steps измеряются loss improvement, loads, две entropy и effective rank. При недостатке улучшения, persistent structural signal и $H<H_{\max}$ выбирается birth rule. Новая gamma начинается с нуля; rate зрелых heads можно уменьшить. Отдельно разрешены pruning/merge и freeze стабильных heads.

Это полезное разделение поведения, но Require не задаёт $\varepsilon_L$, monitoring schedule, длительность persistence, pruning window, contribution metric, validation tolerance, warm-up law и критерий specialisation. Эти элементы нужны для воспроизводимого алгоритма и не могут быть восстановлены из красивой кривой.

## Что следует и чего не следует из guards

Явный $H_{\max}$ ограничивает число heads при росте. Pruning сам по себе не доказывает отсутствие бесконечного чередования birth/death. Низкая load не равна малому functional effect: голова может редко использоваться, но иметь большой output на редких важных данных. Оценка validation loss после tentative removal полезна, однако не даёт uniform гарантии на будущие входы и требует правила отката.

Нулевая gamma не решает изменение общей нормировки gates; этот точный дефект разобран в [345](345-non-destructive-birth-softmax-normalisation-and-width-conflict.md). Поэтому авторская фраза “non-destructive” остаётся условной до согласования самого birth operation.

## Статус рисунков

Источник явно называет Figure 19.8 “synthetic sanity illustrations” и “not benchmark results”. В Figure 19.8 события стоят примерно на шагах 300, 750 и 1150, а в Figure 19.9 — 350, 800 и 1200. Их нельзя читать как единый синхронизированный лог. Loss на рисунке плавно убывает; без threshold/window нельзя независимо установить persistent plateaus.

Figure 19.9 использует ось “normalised diagnostic”, но не задаёт нормировку head entropy; curve превышает 1. Это возможно для raw entropy при достаточно большом $H$, но не для обычной $\mathcal H/\log H\in[0,1]$. Неопределённость сохранена без подгонки рисунка.

Переносимый результат — перечень наблюдаемых величин и предполагаемая логика controller. Утверждения о выигрышах в числе примеров, скорости или качестве требуют реального протокола и сопоставимых измерений; этот книжный проход не проводил обучение моделей.
