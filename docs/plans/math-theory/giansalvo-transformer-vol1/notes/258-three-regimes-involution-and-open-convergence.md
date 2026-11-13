---
title: "258. Три режима, инволюция и открытые переходы от спектра к сходимости"
source_pdf_pages: [648, 649]
book_pages: [612, 613]
batch: "013"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 258. Три режима, инволюция и открытые переходы от спектра к сходимости

## Вопрос

Достаточно ли одной и той же инволюции для одинакового числа голов в expansion, stabilisation и architecture initialisation?

Conjecture 16.56 утверждает инвариантность $K^\star$ между тремя режимами при uniform involution. Алгебраическое условие действительно обеспечивает парность signed spectrum, но само по себе не фиксирует eigenvalue magnitudes, rank, stopping threshold или обучаемую траекторию. Следовательно, переход от инволюции к одинаковому архитектурному бюджету остаётся содержательной гипотезой. [Conjecture 16.56](../source/0647-0650-ch16-architecture-initialisation-and-rank-adaptive-deflation.md#conjecture-16-56).

## Корректное достаточное условие на исходные матрицы

Формула (16.80) требует ортогональную инволюцию $J$ такую, что

$$
J\Sigma_XJ^\top=\Sigma_X,\qquad JK_hJ^\top=-K_h.
$$

Отсюда действительно

$$
J[\Sigma_X,K_h]J^\top
=[J\Sigma_XJ^\top,JK_hJ^\top]
=[\Sigma_X,-K_h]=-[\Sigma_X,K_h].
$$

Это корректный расчёт PDF 649. Он устанавливает условное pairing без необходимости предполагать PSD у самого коммутатора. [Условие (16.80)](../source/0647-0650-ch16-architecture-initialisation-and-rank-adaptive-deflation.md#equation-16-80), [расчёт](../source/0647-0650-ch16-architecture-initialisation-and-rank-adaptive-deflation.md#display-0649-involution-sufficiency-calculation).

В базисе собственных подпространств инволюции $J=\operatorname{diag}(I_p,-I_q)$ эти условия имеют конкретный вид:

$$
\Sigma_X=\begin{pmatrix}S_+&0\\0&S_-\end{pmatrix},\qquad
K_h=\begin{pmatrix}0&L\\-L^\top&0\end{pmatrix}.
$$

Тогда

$$
A_h^{\mathrm{sig}}=\begin{pmatrix}0&D\\D^\top&0\end{pmatrix},
\qquad D=\tfrac12(S_+L-LS_-).
$$

Его ненулевые eigenvalues равны $\pm\sigma_j(D)$, а rank — $2\operatorname{rank}(D)$. Это полезная проверяемая структура: covariance сохраняет два подпространства, skew-часть соединяет только разные подпространства. Источник не доказывает сохранение такой структуры обычной обучаемой траекторией.

## Почему pairing не фиксирует count

Если $A$ удовлетворяет $JAJ^\top=-A$, то $cA$ удовлетворяет тому же равенству для любого $c>0$. При таком масштабировании $\Gamma_0^\pm$ и $\delta_0^\pm$ умножаются на $c$, а $\kappa_{A,\pm}$ не меняются. Правая часть авторской формулы бюджета при фиксированном пороге меняется на

$$
K_\pm^*(cA)-K_\pm^*(A)=\kappa_{A,\pm}^2\log c.
$$

Парность при этом остаётся точной, её ошибка равна нулю. Поэтому даже в собственной формуле источника одного условия инволюции недостаточно для инвариантности head count: нужна связь масштабов, нормировок, порогов и residual dynamics между режимами. Это не отрицание возможной более сильной теоремы, а выделение недостающих предпосылок.

Аналогично, малая $\Delta_{\max}$ из пяти пар не даёт оценки ошибки целочисленного count: около порога сколь угодно малое спектральное возмущение может изменить решение о дополнительной голове. Потребуются полный спектральный контроль и margin до порога, а не только empirical pairing discrepancy.

## Что явно оставлено открытым

На PDF 649 автор выделяет три незакрытых направления: существование структурной инволюции на реальных траекториях; отображение $A_h^{\mathrm{sig}}\to A_{\mathrm{res}}^{(k)}$ с проекцией, signed decomposition, aggregation и совместимостью дефляции; сходимость level-two controller по четырём свойствам — diagnostic stability, coverage-residual decrease, anchor preservation, finite termination. Ссылки «Open Question 13.x» и «13.x+1» — незавершённые placeholders, которые сохранены в source. [§16.13.7](../source/0647-0650-ch16-architecture-initialisation-and-rank-adaptive-deflation.md#section-16-13-7).

Заявленная Lyapunov contraction пока лишь ожидается. Даже доказанное уменьшение score $S_h$ не автоматически означало бы уменьшение task loss или coverage residual: score объединяет amplitude, текущий radius, redundancy и quality, и изменение любого множителя способно изменить его без улучшения остальных величин. Нужна связь выбранного функционала с конкретной целью.

В индексе проекта стоит раздельно хранить доказанную линейную алгебру (16.73–16.76,16.80), определение score, ограниченные наблюдения и conjectures 16.48/16.51–16.56. Их объединение в один ярлык «доказанная архитектурная теория» скрывало бы именно те переходы, которые сам источник оставляет для Volume II.
