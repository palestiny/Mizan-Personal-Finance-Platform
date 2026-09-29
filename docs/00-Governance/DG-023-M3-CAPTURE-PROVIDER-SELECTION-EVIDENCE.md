# DG-023 — Capture Provider Selection & Evidence Gate

**Status:** Proposed — Product Owner decision required  
**Date:** 2026-09-28  
**Phase:** M3 — Frictionless Capture  
**Decision:** Pending evidence-based evaluation

## Problem

DG-022 defines how a real external capture provider may enter Mizan without becoming a financial authority, but it intentionally does not select a vendor.

The next decision must therefore be evidence-based. Mizan needs capture quality across text, receipt/image, and voice channels while preserving the same vendor-neutral `CaptureInterpretation` contract.

Current public documentation shows that provider capabilities differ materially by modality. For example, Azure Document Intelligence provides a GA receipt model for structured receipt extraction, while Gemini provides structured JSON output against a supplied schema. These capabilities are evidence inputs, not proof that either provider is the correct Mizan production choice.

## Proposed decision

Evaluate providers by capability and by the Mizan capture benchmark before locking a production vendor.

Do **not** introduce multi-provider runtime routing yet.

The intended initial production shape is:

`Text / Receipt / Voice → Provider Adapter → CaptureInterpretation → deterministic validation → Context Resolution → Proposal → Confirmation → Financial Command`

One provider may initially serve multiple channels where evidence supports it. The adapter boundary must still allow a future capability-specific provider without changing Domain, Proposal, or Financial Command semantics.

## Evaluation capabilities

### 1. Text capture

Evaluate:

- Arabic input quality
- English input quality
- mixed Arabic/English input
- amount extraction and normalization
- currency extraction
- operation-type interpretation
- effective date/time extraction
- semantic account-reference extraction
- missing-field detection
- ambiguity detection
- contradictory-input handling
- structured-output reliability

### 2. Receipt / image capture

Evaluate:

- merchant extraction
- total amount
- currency
- transaction date/time where available
- tax/fee fields where available
- line-item extraction as a secondary capability
- photographed, scanned, printed, and difficult-quality receipts
- Arabic and mixed-language receipts where representative samples exist

Azure Document Intelligence currently documents a GA receipt model that extracts receipt fields such as merchant, dates, line items, and totals and returns structured data. This makes it a relevant evaluation candidate for receipt capability, but not a preselected production vendor.

### 3. Voice capture

Evaluate:

- Arabic speech transcription
- English speech transcription
- mixed-language speech
- amount recognition
- currency recognition
- operation intent
- account-reference recognition
- missing/ambiguous information
- noisy/background-audio behavior

Voice output must converge into the same vendor-neutral interpretation boundary. Voice transcription itself must not become financial authority.

## Benchmark design

Use a controlled Mizan benchmark before production selection.

Initial target set:

- 30 Arabic text captures
- 20 English text captures
- 10 mixed-language text captures
- 20 receipt images
- 10 ambiguous captures
- 10 incomplete captures
- 10 malformed/adversarial captures

Voice cases should be added before selecting a provider for production voice capture; the benchmark must contain representative Arabic, English, mixed-language, noisy, incomplete, and ambiguous recordings.

The benchmark must use synthetic or explicitly permitted test data by default. Real financial data must not be copied into provider evaluation without an explicit privacy/data-handling decision.

## Measurements

Record at minimum:

1. semantic interpretation accuracy
2. amount accuracy
3. currency accuracy
4. effective date/time accuracy
5. operation-type accuracy
6. account-reference accuracy
7. missing-field detection accuracy
8. ambiguity detection accuracy
9. contradictory-input rejection
10. structured-output validity
11. latency
12. timeout/failure behavior
13. provider retry behavior
14. privacy/data-retention constraints
15. estimated cost per 1,000 captures
16. Arabic-language quality

Accuracy must be measured against a human-authored expected interpretation, not provider confidence.

## Safety and authority rules

- Provider confidence is diagnostic only.
- A high-confidence provider result is never equivalent to user confirmation.
- Provider output may contain semantic account references but never authoritative internal AccountIds.
- Invalid or contradictory output fails closed.
- Missing or ambiguous required information cannot become a confirmable Proposal.
- Provider retries must remain outside the financial command boundary.
- No provider evaluation may create Operations or Effects.
- Raw capture/provider payloads should not be retained by default.
- Provider SDK types remain isolated in Infrastructure.
- Existing Proposal and Financial Command contracts remain unchanged.

## Selection approach

Use evidence to select a production path rather than choosing a vendor from feature lists alone.

Preferred initial architecture:

- one production provider path where practical;
- provider-neutral adapter contract retained;
- capability-specific adapters permitted later;
- no automatic runtime fallback/multi-provider orchestration in M3;
- a second provider may be evaluated as a contingency without introducing runtime routing.

A provider should not be selected merely because it has the broadest feature list. Selection requires acceptable quality on Mizan's benchmark, operational behavior, privacy/data handling, integration fit, and cost.

## Trade-offs

### One provider across channels

**Benefits**

- simpler operations
- fewer integrations
- lower initial implementation complexity
- unified credentials/monitoring

**Costs**

- a single provider may be weaker for a specific modality
- provider capability changes can affect multiple channels

### Best provider per capability

**Benefits**

- can optimize text, receipt, and voice independently
- reduces dependence on one vendor

**Costs**

- more adapters and operational complexity
- more contracts, monitoring, credentials, and failure modes
- higher maintenance burden

### Proposed M3 position

Start with one provider path where the benchmark supports it, while preserving the existing provider-neutral boundary so capability-specific providers can be introduced later without changing financial semantics.

This is an architecture default, not a vendor selection.

## Acceptance criteria

DG-023 is accepted only when:

- benchmark cases and expected interpretations are defined;
- candidate providers/capabilities are documented;
- measured results are recorded;
- privacy/data-handling constraints are reviewed;
- cost and latency are measured or explicitly bounded;
- provider failure behavior is tested;
- a production provider choice, or an explicit decision to defer selection, is recorded;
- no vendor becomes authoritative over financial state.

## Non-goals

- autonomous financial decisions
- autonomous Proposal confirmation
- provider confidence thresholds that authorize execution
- multi-provider runtime fallback
- agent/RAG orchestration
- new financial semantics
- provider-specific types in Domain or Proposal persistence
- importing real personal financial data into benchmarks without an explicit privacy decision
- selecting a vendor solely from marketing claims

## Evidence note

This gate is intentionally an evidence gate. Current official documentation confirms that Azure Document Intelligence has a GA receipt model and that Gemini supports schema-constrained structured output, but those facts alone do not establish suitability for Mizan's Arabic/global capture workload. Provider selection remains pending benchmark evidence.
