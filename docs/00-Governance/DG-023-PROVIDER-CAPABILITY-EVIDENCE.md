# DG-023 — Candidate Capability Evidence Matrix

**Status:** Evidence collected — not a provider selection.

## Purpose

Record current public capability evidence before running the Mizan benchmark. Public documentation establishes whether a candidate is technically testable; it is not treated as proof of benchmark quality.

| Candidate / capability | Relevant documented capability | Mizan relevance | Benchmark question |
|---|---|---|---|
| Google Gemini API | Structured output can be constrained to a developer-defined schema. | Useful for text and potentially multimodal interpretation through one provider adapter. | Does normalized output reliably capture Arabic/mixed financial semantics and correctly surface missing/ambiguous/contradictory information? |
| Azure AI Document Intelligence — Receipt | Receipt model extracts merchant, dates, totals and other receipt fields and returns structured data. v4.0 is documented as GA. | Strong candidate class for receipt-specific extraction. | How accurately does it extract the fields Mizan actually needs from synthetic receipt cases, including degraded images? |
| Azure AI Speech | Speech-to-text documentation lists ar-EG support and custom speech capabilities for Egyptian Arabic. | Candidate for the voice channel where transcription quality is the first boundary. | How accurately does transcription preserve amount, currency, operation and semantic account references in Egyptian Arabic and mixed speech? |

## Evidence interpretation

### Gemini

Official documentation demonstrates schema-constrained structured output. This establishes integration feasibility for the provider-neutral contract, but does not establish Arabic financial accuracy, contradiction detection, or safe handling of Mizan-specific semantics.

### Azure Document Intelligence

Official documentation describes a GA receipt model with structured receipt extraction. It is therefore a concrete specialized receipt candidate. Its documented receipt capability must still be benchmarked against Mizan's synthetic cases and language/format requirements.

### Azure Speech

Official documentation lists Egyptian Arabic (ar-EG) support for speech-to-text and custom speech. This establishes locale availability, not application-level financial interpretation accuracy.

## Current conclusion

No provider is selected by this document.

The benchmark should treat capabilities separately:

1. text interpretation / structured semantics;
2. receipt extraction;
3. voice transcription and downstream interpretation.

A single-provider path may still be evaluated, but specialization should remain possible without changing the Application financial boundary.

## Required next evidence

Before DG-023 can be closed:

- expand the synthetic dataset to the required case counts;
- define representative receipt-image fixtures;
- define representative voice fixtures;
- execute the same evaluation protocol against each candidate;
- record raw benchmark measurements and failure categories;
- review privacy/data-processing terms for the exact deployment configuration;
- record measured or reproducible cost and latency observations;
- make the production-provider decision only after evidence review.

## Sources

- Google Gemini structured output: https://ai.google.dev/gemini-api/docs/get-started
- Azure Document Intelligence receipt model: https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/prebuilt/receipt
- Azure Speech language support: https://learn.microsoft.com/en-us/azure/ai-services/speech-service/language-support
