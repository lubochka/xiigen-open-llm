---
title: "315. Protein MLM: знак asymmetry loss, домен residual и условия Level-3 trigger"
source_pdf_pages: [735, 736]
book_pages: [699, 700]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 315. Protein MLM: знак asymmetry loss, домен residual и условия Level-3 trigger

## Вопрос

Действительно ли напечатанный $L_\alpha$ направляет encoder к порогу роста глубины, как утверждает Proposition 18.2?

**При обычной минимизации записанный hinge действует в противоположную сторону.** Это существенное расхождение между формулой и предлагаемым объяснением эксперимента. Его нельзя устранить заменой знака в source: код и внешний proof здесь не представлены, поэтому намерение автора остаётся неустановленным. [Формулы (18.1–18.2)](../source/0735-0738-ch18-proteome-mlm-mechanism-and-ablation.md#equation-18-1), [Proposition 18.2](../source/0735-0738-ch18-proteome-mlm-mechanism-and-ablation.md#proposition-18-2).

## Что означает асимметрия

Для ненулевой вещественной матрицы $M=S+K$, $S^\top=S$, $K^\top=-K$, Frobenius-ортогональность даёт $\|M\|_F^2=\|S\|_F^2+\|K\|_F^2$. Поэтому

$$
\alpha=\frac{\|K\|_F^2}{\|M\|_F^2}\in[0,1]
$$

— корректная доля skew-energy. При $M=0$ возникает $0/0$, для которого нужен явный convention. Высокая $\alpha$ означает преобладание skew-компоненты $W_QW_K^\top$; она сама по себе не равна насыщению внимания, полезности нового слоя или task residual.

Источник задаёт $L_\alpha=\sum[\alpha-\alpha_{\mathrm{crit}}+\varepsilon_\alpha]_+$, $\alpha_{\mathrm{crit}}=0.9$, $\varepsilon_\alpha=0.01$, $\lambda_\alpha=10$. Один член равен $[\alpha-0.89]_+$. Он нулевой при $\alpha\le0.89$ и возрастает выше этой точки: значения при $\alpha=0.90,0.91,0.989$ равны $0.01,0.02,0.099$.

Если рассматривать только этот член и параметры $\vartheta$, то в гладкой активной области gradient descent даёт

$$
\alpha(\vartheta-\eta\lambda_\alpha\nabla\alpha)
=\alpha(\vartheta)-\eta\lambda_\alpha\|\nabla\alpha\|^2+O(\eta^2).
$$

Таким образом, его непосредственный вклад снижает $\alpha$. Другие losses способны изменить суммарную динамику, но из (18.2) не следует заявленное восстановление $\alpha\ge0.91$. Возможный hinge для поощрения такого порога выглядел бы $[0.91-\alpha]_+$; это **редакторский вариант направления**, не установленное исправление реализации и не самостоятельная гарантия сходимости.

## Почему Proposition 18.2 пока не даёт готового theorem contract

Константа $\bar\alpha_{\mathrm{geo}}$ и нижняя граница $\lambda_\alpha$ названы explicit, но в этих страницах не выписаны. Proof перенесён во внешнюю работу, а ссылка на Chapter 17 содержит “Proposition ??”. Начальные условия и полный $L_{\mathrm{geo}}$ также недостаточны для проверки uniform-in-time ограничения. Один слой сам по себе не исключает $\alpha(0)=1$: ненулевой skew $M$ уже даёт это значение.

Есть и различие кванторов. Trigger наблюдает **максимум в самом глубоком слое на всём окне из 200 шагов**, а proposition говорит о supremum по всем слоям и головам. Большая асимметрия старого нижнего слоя не гарантирует условие на новом верхнем. Название “Level-2 head-growth” в proposition также расходится с Level-1 на соседней странице; source сохраняет оба обозначения.

## Centered attention не становится симметричным оператором

На PDF 735 используется $A_{\mathrm{res}}=A-\mathbf1\bar a^\top$ и trigger $\lambda_{\max}(A_{\mathrm{res}})>0.10$. У обычной attention matrix $A$ нет автоматической симметрии. Если $P$ — циклическая перестановка трёх координат, а $J=\mathbf1\mathbf1^\top$, то строго положительная row-stochastic матрица

$$
A=0.9P+0.1J/3
$$

допустима как row-softmax от $\log A$. Её centered residual равен $0.9(P-J/3)$ и имеет eigenvalues $0,-0.45\pm0.7794228634i$. Значит, обычное упорядоченное вещественное $\lambda_{\max}$ здесь не определено. Для переноса symmetric-gate theory надо явно выбрать симметризацию, singular-value criterion либо другой operator; это разные определения.

Наконец, Gaussian perturbation масштаба $10^{-3}$ обычно обеспечивает лишь приближённую, а не точную identity insertion. Точная policy continuity позднее связана с нулевой выходной проекцией. Эти различия важны для проекта: source-reported depth growth может быть наблюдением, но его объяснение требует согласовать формулу regulariser, operator type, окно trigger и точное правило вставки.
