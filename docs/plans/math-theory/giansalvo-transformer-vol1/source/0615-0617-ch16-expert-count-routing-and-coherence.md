---
title: "Chapter 16: expert-count bounds, routing gates, and coherence"
source_pdf_pages: [615, 617]
book_pages: [579, 581]
batch: "013"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: true
---

# Chapter 16: expert-count bounds, routing gates, and coherence

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: input-conditional complexity](0612-0614-ch16-continual-operation-and-input-conditional-complexity.md) · [Next: transfer overlap and architectural distance](0618-0621-ch16-transfer-overlap-and-architectural-distance.md#pdf-0618)

> Editorial note: Full transcription of PDF 615–617 (printed pages 579–581), beginning with the continuation of Definition 16.21. The Jensen claim, expert-count inequalities, routing objective, coherence assertion, table values, and citation placeholders are preserved as written. Table 16.3 is a transcription of the original table, not an independently verified evaluation of deployed systems. PDF 617 ends after the final paragraph of §16.6.5.

<a id="pdf-0615"></a>
## PDF 615 · Book page 579

*task complexity index* of an input-conditional task is

<a id="equation-16-32"></a>
$$
\bar\kappa_A=\frac{\mathbb E_x[\Gamma_0(x)]}{\mathbb E_x[\delta_0(x)]}.
\tag{16.32}
$$

The pointwise directional task complexity index $\kappa_A(x)=\Gamma_0(x)/\delta_0(x)$ is, in general, larger than $\bar\kappa_A$: a worst-case input forces the architecture to provision capacity it never needs on average. The next lemma quantifies the gap.

<a id="lemma-16-22"></a>
**Lemma 16.22 (Pointwise versus expected).** For any input-conditional task operator $\mathcal B(x)$ with $\Gamma_0(x),\delta_0(x)>0$,

<a id="equation-16-33"></a>
$$
\bar\kappa_A\le\mathbb E_x[\kappa_A(x)],
\tag{16.33}
$$

with equality if and only if $\mathcal B(x)$ is constant in $x$.

*Proof.* The bound is a direct application of Jensen's inequality to the convex function $f(a,b)=a/b$ on $\mathbb R\times\mathbb R_+$, applied to the random variables $\Gamma_0(x)$ and $\delta_0(x)$. The strict inequality holds whenever $\mathcal B(x)$ is genuinely input-conditional, which is the regime of interest. $\square$

The strict inequality is the source of the MoE gain: whenever the task operator varies meaningfully with the input, an architecture that routes can be smaller than one that does not, because the relevant complexity measure is $\bar\kappa_A$ and not the worst-case $\sup_x\kappa_A(x)$.

<a id="section-16-6-2"></a>
### 16.6.2 The optimal expert count

The compressed-sensing bound of the theorem stated in this part, stated for a fixed task operator, generalises directly to the input-conditional setting. The architecture must reduce the residual energy to below $\theta_w$ on average over the input distribution, not on every input separately. This averaging replaces the worst-case directional task complexity index with the expected one.

<a id="theorem-16-23"></a>
**Theorem 16.23 (Optimal expert count).** Let $\mathcal B(x)$ be an input-conditional task operator with $\mathbb E_x[\Gamma_0(x)]=\Gamma_0$ and bounded $\delta_0(x)\ge\delta_0>0$ pointwise. Consider a routing strategy that activates $k$ experts per input out of a pool of $E$ available experts, each grown by INCRT on the input subset routed to it. The smallest expert pool that achieves $\mathbb E_x[\Gamma_{\mathrm{res}}(x)]\le\theta_w$ satisfies

<a id="equation-16-34"></a>
$$
\frac1{2k}\bar\kappa_A^2\log\frac{\Gamma_0}{\theta_w}
\le E^*
\le\frac1k\bar\kappa_A^2\log\frac{\Gamma_0}{\theta_w}.
\tag{16.34}
$$

*Proof sketch.* The upper bound is obtained by applying the theorem stated in this part to each expert in isolation: an expert that receives a fraction $1/E$ of the input mass under uniform routing must reduce the conditional residual on its slice, requiring $\kappa_A^2\log(\Gamma_0/\theta_w)/k$ heads (because $k$ experts share the deflation work per input). Summing over the $E$ experts and bounding the sum by the expectation yields the upper bound. The lower bound follows by constructing a hard family of input-conditional operators for which fewer than half this many experts cannot capture the expected directional energy; the construction parallels the proof of the theorem stated in this part adapted to the input-conditional setting. $\square$

The structure of the bound is identical to the static one: the square of the directional task complexity index, multiplied by the log-ratio of energy to threshold. The only difference is the replacement of $\kappa_A$ by $\bar\kappa_A$, and the additional factor $1/k$ that accounts for the simultaneous activation of $k$ experts per input.

<a id="pdf-0616"></a>
## PDF 616 · Book page 580

**The activation budget.** For a fixed compute budget $C$ proportional to $k$ (each input activates $k$ experts, each contributing equally to inference cost), the bound implies that the total parameter count $E^*\cdot\text{(parameters per expert)}$ scales as $\bar\kappa_A^2/C$. A higher activation budget reduces the required pool size in inverse proportion — the trade-off between width (large $E$) and activation (large $k$) is therefore made explicit by the formula.

<a id="section-16-6-3"></a>
### 16.6.3 Calibration on deployed architectures

The bound of the theorem stated in this part can be evaluated on deployed MoE architectures by estimating $\bar\kappa_A$ from the activation patterns observed during training. For each architecture in the table, the estimated $\bar\kappa_A$ from open-source training runs and the predicted optimal expert count $E_{\mathrm{pred}}^*$ are computed and compared with the deployed $E_{\mathrm{dep}}$.

<a id="table-16-3"></a>
| Architecture | $E_{\mathrm{dep}}$ | $k_{\mathrm{dep}}$ | $\widehat{\bar\kappa}_A$ | $E_{\mathrm{pred}}^*/E_{\mathrm{dep}}$ |
|---|---:|---:|---:|---:|
| Mixtral 8×7B | 8 | 2 | 3.8 | 0.94 |
| DeepSeek-V2 (236B) | 160 | 6 | 11.2 | 1.02 |
| GLaM (1.2T) | 64 | 2 | 7.1 | 1.07 |
| Switch Transformer (1.6T) | 2048 | 1 | 31.4 | 0.91 |
| OLMoE 1B-7B | 64 | 8 | 7.5 | 0.98 |
| Qwen3-235B-A22B | 128 | 8 | 9.4 | 1.03 |

**Table 16.3:** Predicted versus deployed expert counts.

*$\widehat{\bar\kappa}_A$ is the estimated expected directional task complexity index from the architectures' training-time activation logs; $E_{\mathrm{pred}}^*$ is computed from the theorem stated in this part with $\theta_w$ matched to the training-loss target. The ratio $E_{\mathrm{pred}}^*/E_{\mathrm{dep}}$ is between $0.91$ and $1.07$ across all configurations, within the predicted constant factor of two of the theorem.*

The agreement is striking. Across architectures spanning three orders of magnitude in expert count and four orders of magnitude in total parameters, the predicted optimal $E_{\mathrm{pred}}^*$ matches the deployed $E_{\mathrm{dep}}$ within seven percent. The empirical configurations chosen by industrial practice — through extensive ablation, scaling law fitting, and engineering intuition — are the same as those derived from the spectral directional task complexity index of the training distribution.

<a id="section-16-6-4"></a>
### 16.6.4 Routing as an input-conditional gate

The routing function $r:\mathcal X\to\Delta_E$ assigns each input to a probability distribution over the $E$ experts, with the top-$k$ entries activated. The choice of $r$ has been studied extensively [citation], with proposals ranging from learned softmax gating to expert-choice routing to hash-based assignment.

The framework gives a structural characterisation.

<a id="theorem-16-24"></a>
**Theorem 16.24 (Routing-gate equivalence).** Let $\{u_e^+\}_{e=1}^E$ be the leading captured eigenvectors of the per-expert residual operators $A_{\mathrm{res}}^{(e)}(x)$, and define the per-expert Rayleigh quotient $R_e^+(x)=u_e^{+\top}A_{\mathrm{res}}^{(e)}(x)\,u_e^+$. The routing function that minimises the expected residual under top-$k$ activation is

<a id="equation-16-35"></a>
$$
r^*(x)_e
=\mathbb1\bigl[e\in\operatorname{top-}k(\{R_e^+(x)\}_{e=1}^E)\bigr].
\tag{16.35}
$$

*Proof sketch.* The expected residual under routing $r$ is $\mathbb E_x[\sum_e r(x)_e\,\Gamma_{\mathrm{res}}^{(e)}(x)]$, where $\Gamma_{\mathrm{res}}^{(e)}(x)$ is the residual contributed by expert $e$ on input $x$. Minimising over $r$ subject to the top-

<a id="pdf-0617"></a>
## PDF 617 · Book page 581

$k$ activation constraint is a linear assignment problem: the optimal routing assigns each input to the $k$ experts whose conditional residual energy is largest. The Rayleigh quotient $R_e^+(x)$ is precisely this energy by the spectral characterisation of $\lambda_1(A_{\mathrm{res}})$, completing the identification. $\square$

The routing function is therefore not a separate component to be designed; it is the bilateral gate of INCRT I applied input-conditionally. The same gate that signals when growth is needed in the static setting signals which expert to activate in the routing setting. The two roles are identical at the spectral level — the gate ranks subspaces by their residual energy, and the architectural response (grow a head, route to an expert) depends on the configuration of the architecture above the gate.

**Connection to existing routing strategies.** Top-$k$ softmax routing [citation] approximates $r^*$ by a softmax over a learned linear projection of $x$. The framework predicts that this approximation is tight when the projection learns to align with the dominant eigenvectors of the expert residuals. Expert-choice routing [citation] reverses the quantification (each expert chooses its inputs), which corresponds to the dual of the assignment problem. Hash-based routing replaces $r^*$ with a fixed coordinate-free assignment, which the framework predicts is suboptimal except in the regime where $\mathcal B(x)$ is approximately constant in $x$.

<a id="section-16-6-5"></a>
### 16.6.5 The orthogonality regime for experts

A practical concern in MoE architectures is the redundancy between experts: when expert representations have high mutual coherence, the routing strategy struggles to recover the optimal expert subset, and the architecture wastes capacity. The phenomenon has been observed empirically and analysed theoretically [citation], with the conclusion that imposing geometric orthogonality between expert representations narrows the gap between greedy routing and the (NP-hard) global optimum.

The framework derives the orthogonality requirement from the spectral structure of the task, rather than imposing it externally. By the lemma stated above (exact deflation), each new head within a single architecture is, by construction, orthogonal to the existing ones — the deflation step projects the residual onto the orthogonal subspace before the next growth direction is selected. The same principle applied across experts, rather than within a single one, gives the architectural orthogonality regime.

<a id="corollary-16-25"></a>
**Corollary 16.25 (Coherence regime for experts).** When the captured subspaces $\operatorname{span}\{u_e^+\}_{e=1}^E$ have mutual coherence $\delta<1/E^*$, the bound of the theorem stated in this part holds with the constant of the theorem. When $\delta\ge1/E^*$, the bound degrades to the random-growth regime of the theorem stated in this part, with $E_\delta^*=\Theta(d/\theta_w^2)$ instead of $\bar\kappa_A^2$.

*Proof.* Direct combination of the theorem stated in this part, applied to the union of expert subspaces, with the bound of the theorem. $\square$

The corollary states that the orthogonality regime — the regime in which the bound is tight — is reached when expert subspaces do not overlap above a precise threshold. The orthogonality regulariser proposed in [citation] is, in the present framework, the explicit enforcement of the deflation condition. The framework recovers the same prescription via a structural argument rather than a regularisation argument: the experts are orthogonal by construction when each is grown by the deflation mechanism on its own input subset.
