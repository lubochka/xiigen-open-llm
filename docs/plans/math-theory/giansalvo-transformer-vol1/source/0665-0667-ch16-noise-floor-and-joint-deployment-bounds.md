---
title: "Chapter 16: quantisation noise floor and joint deployment bounds"
source_pdf_pages: [665, 667]
book_pages: [629, 631]
batch: "014"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: true
---

# Chapter 16: quantisation noise floor and joint deployment bounds

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: quantisation model](0662-0664-ch16-minimum-budget-dual-and-quantisation-model.md) · [Next: deployment frontier and implicit task operator](0668-0670-ch16-deployment-frontier-and-implicit-task-operator.md#pdf-0668)

> Editorial note: Full transcription of PDF 665–667 (printed pages 629–631). The chain-rule argument, noise-floor claim, maximum composition rule, dimensional conventions, and universal-floor conjecture are preserved in their original form. The excerpt ends immediately before the empirical table introduced by the last sentence of PDF 667; that table belongs to PDF 668.

<a id="pdf-0665"></a>
## PDF 665 · Book page 629

*Proof.* Linearising the task operator around the unquantised parameters gives

<a id="equation-16-108"></a>
$$
\mathcal B(\phi-\Delta\phi)
=\mathcal B(\phi)-J(\Delta\phi)+O(\|\Delta\phi\|^2),
\qquad J(\Delta\phi)=\sum_\ell J^{(\ell)}\cdot\Delta W^{(\ell)}.
\tag{16.108}
$$

The remainder is $O(4^{-b})$ and is absorbed into the $(1+O(2^{-b}))$ factor. By independence and zero mean, cross-layer terms vanish in expectation. For each layer,

<a id="equation-16-109"></a>
$$
\begin{aligned}
\mathbb E[\|J^{(\ell)}(\Delta W^{(\ell)})\|_F^2]
&=\sum_{p,q,i,j,i',j'}
J^{(\ell)}_{(p,q),(i,j)}
J^{(\ell)}_{(p,q),(i',j')}
\mathbb E[\Delta W_{ij}^{(\ell)}\Delta W_{i'j'}^{(\ell)}]\\
&=\frac{(R^{(\ell)})^2}{3\cdot4^b}
\sum_{p,q,i,j}\bigl(J^{(\ell)}_{(p,q),(i,j)}\bigr)^2.
\end{aligned}
\tag{16.109}
$$

Summing over $\ell$ proves (16.107). $\square$

<a id="lemma-16-72"></a>
**Lemma 16.72 (Quantisation noise floor).** Suppose (Q1)–(Q3) hold. For any architecture trained to minimise the dimensionless residual energy

<a id="display-0665-dimensionless-residual-energy"></a>
$$
\Gamma_{\mathrm{res}}=\|A_{\mathrm{res}}\|_F^2/\Gamma_0^2
$$

with respect to the infinite-precision task operator $\mathcal B$, the quantised architecture satisfies

<a id="equation-16-110"></a>
$$
\mathbb E[\Gamma_{\mathrm{res}}^{\mathrm{quant}}]^{1/2}
\ge\sigma_q(b),
\qquad \sigma_q(b)=\frac{\beta^*}{2^b\sqrt3},
\tag{16.110}
$$

independently of the number of heads, depth, or rank distribution.

*Proof.* The quantised architecture computes with $Q_b(\phi)$ and therefore targets

<a id="display-0665-quantised-task-operator"></a>
$$
\widetilde{\mathcal B}=\mathcal B(Q_b(\phi))
=\mathcal B-\Delta\mathcal B.
$$

At the end of training, its reconstruction approximates $\widetilde{\mathcal B}$, not $\mathcal B$. The residual with respect to the infinite-precision target is

<a id="equation-16-111"></a>
$$
A_{\mathrm{res}}^{\mathrm{quant}}
=\mathcal B-\sum_h\lambda_hu_hu_h^\top
=\underbrace{\widetilde{\mathcal B}
-\sum_h\lambda_hu_hu_h^\top}_{\widetilde A}
+\Delta\mathcal B.
\tag{16.111}
$$

The architecture can reduce $\widetilde A$, but cannot remove $\Delta\mathcal B$, which is created by the representation itself. In the regime where the architectural residual on $\widetilde{\mathcal B}$ is smaller than the quantisation offset,

<a id="display-0665-small-architectural-residual-bound"></a>
$$
\|A_{\mathrm{res}}^{\mathrm{quant}}\|_F
\ge(1-\delta)\|\Delta\mathcal B\|_F
$$

for arbitrarily small $\delta>0$ as the architecture is enlarged. Squaring, taking expectation, and applying (Q3) gives

<a id="display-0665-expected-quantised-residual-bound"></a>
$$
\mathbb E[\|A_{\mathrm{res}}^{\mathrm{quant}}\|_F^2]
\ge(1-\delta)^2
\frac{(\beta^*)^2\Gamma_0^2}{3\cdot4^b}.
$$

Dividing by $\Gamma_0^2$ and letting $\delta\to0$ yields (16.110). $\square$

<a id="remark-16-73"></a>
**Remark 16.73 (Dimensional convention).** The rigorous floor is dimensionless: $\sigma_q(b)$ is the square root of the dimensionless residual energy. The older shorthand $\Gamma_0/(2^b\sqrt3)$ corresponds to the same statement before division by $\Gamma_0$. The factor $\beta^*$ records the architecture-dependent chain-rule gain and is typically of order one.

<a id="pdf-0666"></a>
## PDF 666 · Book page 630

<a id="section-16-16-3"></a>
### 16.16.3 The combined deployment constraint

Both constraints are usually present in deployment. A model must fit within a head budget $K_{\max}$ and use a prescribed bit-width $b$. In dimensionless residual units the budget-only term is $\exp(-K_{\max}/\kappa_A^2)$, while the quantisation term is $\sigma_q(b)$.

<a id="theorem-16-74"></a>
**Theorem 16.74 (Joint deployment bound).** Suppose Theorem 16.69 holds and the quantisation noise floor of Lemma 16.72 applies. The smallest dimensionless residual threshold achievable under $K\le K_{\max}$ and $b$-bit weight quantisation is

<a id="equation-16-112"></a>
$$
\theta_w^*(K_{\max},b)
=\max\left\{\exp\left(-\frac{K_{\max}}{\kappa_A^2}\right),
\frac{\beta^*}{2^b\sqrt3}\right\}.
\tag{16.112}
$$

The transition between the budget-binding and quantisation-binding regimes is

<a id="equation-16-113"></a>
$$
K_{\max}^{\mathrm{trans}}(b)
=\kappa_A^2\bigl(b\log2+\tfrac12\log3-\log\beta^*\bigr).
\tag{16.113}
$$

*Proof.* Any achievable threshold must be at least the budget-only lower bound from Theorem 16.69, after normalising by $\Gamma_0$, and at least the quantisation floor from Lemma 16.72. Hence it is at least the maximum in (16.112). Conversely, in the budget-binding regime the budget-only architecture has residual above the quantisation floor, so quantisation does not change the active constraint. In the quantisation-binding regime, increasing $K$ beyond the transition cannot reduce the residual below $\sigma_q(b)$. The maximum is therefore tight in the two regimes. Equating the two terms gives

<a id="display-0666-budget-quantisation-crossover"></a>
$$
\exp(-K_{\max}/\kappa_A^2)=\beta^*/(2^b\sqrt3),
$$

which is (16.113). $\square$

**The matched regime.** The matched architecture for a given bit-width is the one for which the two constraints are simultaneously tight. For INT8 weights and $\kappa_A=4$, taking the ideal reference $\beta^*=1$ gives $K_{\max}\approx16(8\log2+0.55)\approx98$ heads. A smaller $\beta^*$ shifts this transition by the additive amount $-\kappa_A^2\log\beta^*$.

<a id="section-16-16-4"></a>
### 16.16.4 Quantisation-aware INCRT

The growth threshold should be aware of the quantisation floor. If the target threshold is below $\sigma_q(b)$, additional growth events are wasted because the representation itself prevents further useful reduction.

<a id="corollary-16-75"></a>
**Corollary 16.75 (Quantisation-aware threshold).** Let $\theta_w^{\mathrm{tgt}}\in(0,1]$ be the practitioner's dimensionless target residual threshold. Under the assumptions of Theorem 16.74, set

<a id="equation-16-114"></a>
$$
\theta_w^{\mathrm{aware}}(b)
:=\max\bigl(\theta_w^{\mathrm{tgt}},\sigma_q(b)\bigr),
\qquad \sigma_q(b)=\frac{\beta^*}{2^b\sqrt3}.
\tag{16.114}
$$

Then the resulting head count is

<a id="equation-16-115"></a>
$$
K^*(b)=\min\left\{K^*(\theta_w^{\mathrm{tgt}}),
\kappa_A^2\bigl(b\log2+\tfrac12\log3-\log\beta^*\bigr)\right\}.
\tag{16.115}
$$

Architectures larger than $K^*(b)$ are over-provisioned relative to the quantisation floor.

*Proof.* The INCRT algorithm with threshold $\theta_w^{\mathrm{aware}}(b)$ terminates when its running residual reaches the larger of the target threshold and the quantisation floor. If the target dominates, the ordinary INCRT I count $K^*(\theta_w^{\mathrm{tgt}})$ is recovered. If the floor dominates, substituting $\sigma_q(b)=\beta^*/(2^b\sqrt3)$ into the inverse CS relation gives the second term in (16.115). Taking the minimum of the two cases proves the formula. $\square$

<a id="pdf-0667"></a>
## PDF 667 · Book page 631

<a id="prescription-0667-quantisation-aware-stopping"></a>
**Operational reading.** A practitioner deploying an INCRT-grown architecture under $b$-bit quantisation should compute $\sigma_q(b)$, raise the growth threshold to $\theta_w^{\mathrm{aware}}(b)$ when necessary, and stop growth at $K^*(b)$. Ignoring the floor causes over-provisioning by $K^*(\theta_w^{\mathrm{tgt}})-K^*(b)$ heads in the quantisation-binding regime.

**Connection to GPTQ.** The recent identification of GPTQ [citation] with Babai's nearest-plane algorithm on a Hessian-defined lattice [citation] places the quantisation problem in a geometric setting that parallels the compressed-sensing argument of the present framework. GPTQ minimises the layer-wise reconstruction error subject to $b$-bit constraints; the present framework establishes that the reconstruction error has a lower bound $\sigma_q$ regardless of the algorithm. The two analyses are complementary: GPTQ achieves the floor in practice, the framework explains why no algorithm can go below.

<a id="section-16-16-5"></a>
### 16.16.5 Beyond uniform quantisation

The bound of the lemma stated above assumes uniform quantisation with optimally chosen scale. Modern quantisation methods — AWQ [citation], SmoothQuant [citation], GPTQ [citation], LATMiX [citation] — improve over the uniform baseline by exploiting structure in the weights or activations. The framework's prediction is that all such methods are subject to the same dimensionless noise floor $\sigma_q(b)=\beta^*/(2^b\sqrt3)$, with the constant in front varying by a factor at most $O(1)$.

The reason is structural. The noise floor of the lemma stated above reflects the Shannon information capacity of $b$-bit representations: $b$ bits per weight encode at most $b$ bits per weight, regardless of the quantisation strategy. Methods that exploit weight or activation structure reduce the effective bit-width below $b$ but cannot reduce it below the rate-distortion lower bound for the operator class.

<a id="conjecture-16-76"></a>
**Conjecture 16.76 (Universal noise floor).** For any $b$-bit weight quantisation method that preserves linearity of the attention operator, the residual energy satisfies $\mathbb E[\Gamma_{\mathrm{res}}^{\mathrm{quant}}]^{1/2}\ge c\,\sigma_q(b)$ with a constant $c\in[1/2,1]$ that depends on the quantisation method but not on the task.

The conjecture is consistent with the empirical observations of [citation] that aggressive quantisation methods reach within a small constant factor of the uniform floor across diverse task families, but its rigorous proof requires the rate-distortion analysis of attention operators that lies outside the present framework.

<a id="section-16-16-6"></a>
### 16.16.6 Empirical signature

The two regimes of the theorem stated in this part are testable. For a controlled task with $\kappa_A=4$ and $\Gamma_0=1$, the predicted joint bound, in the ideal reference case $\beta^*=1$, is $\theta_w^*(K_{\max},b)=\max(\exp(-K_{\max}/16),2^{-b}/\sqrt3)$. Five $K_{\max}$ values cross five $b$ values; the predicted binding regime is read off directly from the preceding relation.

**Result.** the table reports the empirically achieved threshold $\widehat\theta_w$ at convergence.
