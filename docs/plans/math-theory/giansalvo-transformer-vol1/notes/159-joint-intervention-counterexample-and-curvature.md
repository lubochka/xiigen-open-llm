---
title: "159. Joint intervention: контрпример к константе 2 и корректный статус curvature bound"
source_pdf_pages: [474, 478]
book_pages: [438, 442]
batch: "010"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 159. Joint intervention: контрпример к константе 2 и корректный статус curvature bound

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Что напечатано

Theorem 13.18 утверждает $\mathcal V_I^{\mathrm{both}}\le2(M/\kappa)(\mathcal V_I^s+\mathcal V_I^a)$ при smooth strongly convex loss и стационарности вдоль вмешательств. Доказательство сначала получает зависящий от values и attention prefactor, затем снова возвращается к универсальной константе 2. Этот переход неверен. [Формулировка](../source/0472-0475-ch13-activation-and-intervention-bounds.md#theorem-13-18), [продолжение proof](../source/0476-0478-ch13-log-centred-subadditivity.md#pdf-0476).

## Точный редакторский контрпример

Возьмём $n=d=d_k=2$, $X=I_2$, $V=I_2$ и

$$
M=\sqrt2\log3\begin{pmatrix}-1&1\\-1&1\end{pmatrix},\quad
M_s=\sqrt2\log3\begin{pmatrix}-1&0\\0&1\end{pmatrix},\quad
M_a=\sqrt2\log3\begin{pmatrix}0&1\\-1&0\end{pmatrix}.
$$

После деления logits на $\sqrt{d_k}$:

$$
A_{\mathrm{full}}=\begin{pmatrix}1/10&9/10\\1/10&9/10\end{pmatrix},\quad
A_{\mathrm{TrA}}=A_{\mathrm{TrB}}=\begin{pmatrix}1/4&3/4\\1/4&3/4\end{pmatrix},\quad
A_{\mathrm{both}}=\begin{pmatrix}1/2&1/2\\1/2&1/2\end{pmatrix}.
$$

Пусть $L(U)=\frac12\|U-A_{\mathrm{full}}\|_F^2$. Точка полного выхода — точный минимум; $M_{\mathrm{smooth}}=\kappa=1$. Прямой расчёт даёт

$$
\mathcal V_I^s=\mathcal V_I^a=9/200=0.045,\qquad
\mathcal V_I^{\mathrm{both}}=8/25=0.32,
$$

тогда как правая часть (13.32) равна $2(0.045+0.045)=0.18$. Отношение равно $32/9>2$. Здесь $V$ имеет condition number 1, все attention entries не меньше $0.1$, loss идеально обусловлена. Поэтому нарушение нельзя объявить исключительно патологией почти нулевого softmax или неинъективности values. Оно сохраняется при достаточно малом perturbation к full-rank $M$; цель loss можно соответственно задать новым полным выходом.

$X=I_2$ совместима и с affine LayerNorm output: нормированные строки $(1,-1)$ и $(-1,1)$ после scale $\gamma=(1/2,1/2)$ и shift $\beta=(1/2,1/2)$ дают $I_2$; регуляризатор LayerNorm можно учесть в scale. Пример — редакторская проверка, не авторский эксперимент.

## Какие оценки остаются допустимы

При инъективности right-multiplication, минимальном entry $\epsilon$ и stationarity логика [158](158-log-centred-softmax-and-value-injectivity.md) даёт

$$
\mathcal V_I^{\mathrm{both}}
\le\frac{M}{\kappa}\frac{\|V\|_{\mathrm{op}}^2}{2\epsilon^2m_V^2}
(\mathcal V_I^s+\mathcal V_I^a).
$$

Это условная, обычно грубая оценка. Дополнительный множитель 2 в напечатанных (13.48) и (13.50) делает их слабее строго выведенной оценки и **сам по себе не является опровержением**. Существенны неоговорённая инъективность, смена пространства и потеря stationarity в Corollary 13.19. Без неё smoothness даёт также линейный член $\langle\nabla L(u),D\rangle$; bare convexity его не обнуляет. [Corollary 13.19](../source/0476-0478-ch13-log-centred-subadditivity.md#corollary-13-19).

Нижняя граница row entropy не эквивалентна нижней границе каждого entry: распределение $(\epsilon,(1-\epsilon)/2,(1-\epsilon)/2)$ имеет entropy, стремящуюся к $\log2$, при исчезающем первом entry. В Table 13.3 уже напечатан max ratio **2.104**, а не универсальный $\rho\le2$. Девять настроек по 500 конфигураций также не совпадают с соседней ссылкой на 2000 samples. Это авторская таблица; её статистические утверждения нельзя превратить в deterministic theorem. [Таблица](../source/0476-0478-ch13-log-centred-subadditivity.md#table-13-3).
