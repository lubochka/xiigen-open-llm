---
title: "Разложение на секторы и приближение к закрытому режиму"
source_pdf_pages: [224, 225]
book_pages: [188, 189]
batch: "005"
language: ru
type: knowledge-note
evidence_status: sufficient-bound-with-editorial-counterexamples-and-global-proof
---

# 50. Секторы билинейной формы и flatness закрытого режима

[Индекс](../README.md) · [Margin и flatness](49-margin-flatness-and-relative-emergence.md) · [Изотропность и silence](51-subspace-activation-and-sequence-silence.md) · [Авторские исправления](52-corrected-completeness-and-emergence-claims.md)

**Закрытый режим определяется равномерными строками post-softmax матрицы.** Он получается из постоянных logits внутри каждой строки. Малость кососимметричного сектора — лишь возможная часть достаточной оценки; она не является определением закрытого режима. Источник верно возвращает условия на уровень logits, но его последнее пояснение о ролях $M_s$ и $M_a$ всё ещё содержит слишком сильное утверждение. [§6.18.4–6.18.5](../source/0221-0224-ch06-margin-flatness-and-relative-emergence.md#section-6-18-4).

## 1. Что непосредственно следует из разложения

При

$$
M=M_s+M_a,\quad M_s^\top=M_s,\quad M_a^\top=-M_a,\qquad
L=\frac{XMX^\top}{\sqrt{d_k}}
$$

формула (6.46) даёт

$$
L_{ij}-L_{ik}
=\frac{x_i^\top M_s(x_j-x_k)}{\sqrt{d_k}}
+\frac{x_i^\top M_a(x_j-x_k)}{\sqrt{d_k}}.
$$

Она верна для любых сравниваемых колонок. Поэтому **оба сектора влияют и на внешний запас $\Delta_i$, и на внутригрупповой разброс $\rho_i$**. Поскольку минимум, максимум и модуль нелинейны, сами $\Delta_i$ и $\rho_i$ нельзя просто разложить в сумму независимо вычисленных секторных индексов; возможны усиление и компенсация. [Формула (6.46)](../source/0221-0224-ch06-margin-flatness-and-relative-emergence.md#equation-6-46).

Для self-attention действительно $x_i^\top M_a x_i=0$. Однако off-diagonal вклад $M_a$ не обязан быть малым: слово «secondary» в авторском объяснении open gate не является количественной оценкой. Для directional gate автор отдельно и правильно уточняет, что Theorem 6.61 **не требует $M_a$-dominance**; ей нужны реальные запасы в пользу выбранной колонки во всех нужных строках.

### Редакторский контрпример к «$M_a$ contributes to $\Delta_i$ rather than $\rho_i$»

Возьмём $d=d_k=2$,

$$
J=\begin{pmatrix}0&1\\-1&0\end{pmatrix},\qquad
M_s=0,\quad M_a=\sqrt2\,J,\qquad
X=\begin{pmatrix}1&0\\0&1\\0&-1\end{pmatrix}.
$$

Для первой строки и $C_1=\{2,3\}$ симметричные affinity одинаковы и равны нулю. Но $L_{12}=1$, $L_{13}=-1$, поэтому $\rho_1=2$. Кососимметричный сектор полностью создаёт внутригрупповую неоднородность. Масштабируя $M_a$, можно сделать её сколь угодно большой. Это опровергает последний bullet §6.18.4, сохраняя саму формулу (6.46).

## 2. Proposition 6.62: что сказано в источнике

Пусть $S_{ME}=\mathbf1\mathbf1^\top/n$ и для некоторой общей константы $\bar\ell$

$$
\|L-\bar\ell\mathbf1\mathbf1^\top\|_\infty\le\varepsilon.
$$

В доказательстве норма используется как максимальное отклонение отдельного элемента. Тогда источник утверждает

$$
\|S-S_{ME}\|_F\le2n\varepsilon,\qquad
d_{\mathrm{closed}}(S)
=\frac{\|S-S_{ME}\|_F}{\sqrt n}
\le2\sqrt n\,\varepsilon.
$$

Это достаточная, довольно консервативная граница. Исходное доказательство пишет «for sufficiently small $\varepsilon$», хотя формулировка не ограничивает $\varepsilon$. Сам результат можно обосновать глобально, без такого ограничения. [Proposition 6.62 и proof](../source/0221-0224-ch06-margin-flatness-and-relative-emergence.md#proposition-6-62).

### Редакторское глобальное обоснование

Для $p=\operatorname{softmax}(z)$ якобиан $J_p=\operatorname{diag}(p)-pp^\top$ симметричен, положительно полуопределён и имеет спектральную норму не больше $1/2$. Например, сумма абсолютных значений элементов его строки равна $2p_j(1-p_j)\le1/2$. Интегрируя якобиан вдоль отрезка, получаем

$$
\|\operatorname{softmax}(z)-\operatorname{softmax}(w)\|_2
\le\frac12\|z-w\|_2.
$$

Построчное применение с $w=\bar\ell\mathbf1$ даёт более сильную оценку

$$
\|S-S_{ME}\|_F
\le\frac12\|L-\bar\ell\mathbf1\mathbf1^\top\|_F
\le\frac n2\varepsilon
\le2n\varepsilon.
$$

Следовательно, напечатанная граница верна для всех $\varepsilon\ge0$ при указанном смысле нормы; ограничение «sufficiently small» — недостаточность приведённого объяснения, а не контрпример к самой границе. Эта более сильная константа — редакторский вывод, не формула книги.

## 3. Общая константа не необходима

Row-softmax инвариантен к добавлению разных констант по строкам:

$$
\operatorname{softmax}_{\mathrm{row}}(L+c\mathbf1^\top)
=\operatorname{softmax}_{\mathrm{row}}(L).
$$

Поэтому для **конечных logits**

$$
S=S_{ME}
\quad\Longleftrightarrow\quad
L=c\mathbf1^\top
\quad\text{для некоторого }c\in\mathbb R^n.
$$

Не требуется $c=\bar\ell\mathbf1$. Например,

$$
L=\begin{pmatrix}0&0\\B&B\end{pmatrix}
$$

имеет точно закрытый softmax при любом $B$, хотя наименьшее отклонение от одной общей константы равно $\lvert B\rvert/2$. Его кососимметричная часть при $B\ne0$ ненулевая. Таким образом, ни общая flatness к одной $\bar\ell$, ни нулевой $M_a$ не являются необходимыми условиями закрытого post-softmax режима.

Практически более естественная достаточная версия разрешает отдельные $c_i$:

$$
\max_{i,j}\lvert L_{ij}-c_i\rvert\le\varepsilon.
$$

Та же глобальная оценка действует без изменений. Альтернативно можно пользоваться построчным spread из [note 49](49-margin-flatness-and-relative-emergence.md), не выбирая $c_i$.

## 4. Исправление коэффициента 2 в секторных бюджетах

На PDF 225 автор явно исправляет предшествующий вариант. Если

$$
\|XM_aX^\top\|_\infty\le\varepsilon_0\sqrt{d_k},\qquad
\|XM_sX^\top-\bar\ell\sqrt{d_k}\mathbf1\mathbf1^\top\|_\infty
\le\varepsilon_0\sqrt{d_k},
$$

то после деления на $\sqrt{d_k}$ и неравенства треугольника **общий** logit budget равен $2\varepsilon_0$. Подстановка в печатную Proposition 6.62 даёт

$$
\|S-S_{ME}\|_F\le4n\varepsilon_0,
$$

а не $2n\varepsilon_0$. Это верная авторская поправка к применению его консервативной константы. Она не утверждает, что $4n$ оптимально. [PDF 225, начало страницы](../source/0225-0233-ch06-activation-corrections-and-synthesis.md#pdf-0225).

И секторные нормы, и margin/spread здесь зависят от **пары $(M,X)$**, не только от параметров головы. Малый $\|M_a\|$ без контроля входных норм не обеспечивает малого вклада в logits; нулевая измеренная кососимметричная часть не обеспечивает равномерности строк. Следующая заметка даёт точный критерий её исчезновения на данном подпространстве, отдельно от закрытого режима.
