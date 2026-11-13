---
title: "347. Новый anchor: residual direction, неопределённый выбор и split"
source_pdf_pages: [778, 779]
book_pages: [742, 743]
batch: "016"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 347. Новый anchor: residual direction, неопределённый выбор и split

## Вопрос

Чем отличаются три способа выбора новой головы и что каждый из них действительно измеряет?

**Residual novelty ищет направление необъяснённых признаков, underserved-token rule ищет неуверенный gate, а split делит загруженный anchor. Эти эвристики не равнозначны и требуют своих условий.**

Источник: [§19.6.4](../source/0777-0779-ch19-incremental-birth-monitoring-and-controller.md#section-19-6-4), [продолжение split на PDF 779](../source/0777-0779-ch19-incremental-birth-monitoring-and-controller.md#pdf-0779).

## Residual novelty

Правило $a_{H+1}=v_1(R)/\|v_1(R)\|$ направляет голову вдоль ведущего residual. Но $R_i$ дан только как пример плохо восстановленного признака; нет единого определения residual objective, centering и используемой матрицы. Если речь о правом singular vector, он имеет нужную размерность feature space; левый singular vector обычно живёт в token space. При нулевом residual или кратном максимальном singular value направление требует отдельного tie/degeneracy policy.

Значит, эта формула — понятная спектральная идея, а не полностью заданный stable implementation. Ведущий residual также может отражать шум или несущественную для loss variation.

## Underserved token

Правило $i^\star=\arg\min_i\max_hg_h(x_i)$ выбирает токен, для которого крупнейшая доля softmax мала, то есть распределение по heads сравнительно равномерно. Оно не измеряет абсолютное расстояние до anchors. Softmax остаётся неизменным при прибавлении одинаковой константы ко всем scores; далёкость/близость в feature space этим критерием не определяется.

При $H=1$ все gates равны 1 и все токены связаны tie. При $x_{i^\star}=0$ нормировка $x/\|x\|$ не определена. Если все anchors дают почти одинаково хорошие scores, правило всё равно может отметить токен как неуверенный. Поэтому “weakest dictionary coverage” — интерпретация эвристики, а не следствие формулы distance.

## DDCL-style split

Split изменяет не только новый anchor, но и старый: $a_{h^\star}\mapsto\operatorname{norm}(a_{h^\star}+\delta v)$. Это сразу меняет gates, даже если новый $\gamma=0$. Поэтому он нуждается в отдельном functional-continuity анализе сверх проблемы общего softmax denominator из [карточки 345](345-non-destructive-birth-softmax-normalisation-and-width-conflict.md).

Нужно определить local variance direction, нормировку, scale $\delta$, perturbations $\epsilon Z$, optimizer state новых параметров и связь with task residual. Figure 19.7 иллюстрирует эту идею схематически; её точки не являются результатом обучающего benchmark. Сам split не доказывает полезность дополнительной ёмкости.
