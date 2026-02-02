## Spec: CohereTutorService - API Contract & Resilience
**File:** StoryFort.Tests.Unit/CohereTutorService_Specs.cs
**Type:** Unit (mock HttpClient) + small integration (in-process HTTP mock)
**Priority:** Critical
**Estimated Tests:** 8

### Context
`CohereTutorService` sends prompts to the external LLM via the named HttpClient "LLM". It must:
- include required non-training headers,
- serialize prompt payload correctly,
- parse Cohere responses (chat-only text and reasoning+text variants),
- handle 401/429/5xx errors and timeouts with defined retry/failure behavior,
- never include sensitive fields in logs.

### Behavior Requirements

#### Happy Path - Chat Model
- GIVEN a valid API key and prompt payload
- WHEN `SendPromptAsync` is called and the LLM returns a normal chat response JSON
- THEN the service returns the extracted assistant text and no error

#### Happy Path - Reasoning Model
- GIVEN a reasoning-style response containing `thoughts` + `output`
- WHEN parsed
- THEN both `thoughts` (optional) and `output` are returned/mapped to result fields

#### Unauthorized
- GIVEN the LLM returns 401
- WHEN `SendPromptAsync` is called
- THEN the service surfaces an UnauthorizedException (or typed failure) without logging the API key

#### Rate Limit
- GIVEN the LLM returns 429 with `Retry-After`
- WHEN called
- THEN the service respects `Retry-After` (one retry in unit test) and ultimately fails if limit persists

#### Timeout / Network Failure
- GIVEN a transient network timeout
- WHEN called
- THEN the service retries per policy (configurable) and fails gracefully if retries exhausted

#### Malformed JSON
- GIVEN LLM returns invalid JSON
- WHEN parsed
- THEN the service returns a parse error (graceful failure) and does not throw an unhandled exception

#### Headers & Non-Training Consent
- GIVEN any request to the LLM
- WHEN sending the HTTP request
- THEN request headers include `X-Client-Name: StoryFort` and the non-training/opt-out header per requirements

### Non-Functional
- Security: do not log full prompt or API key
- Performance: calls should complete within configured timeout; retries limited
- Test approach: unit mocks for error & parse cases; small integration test asserting headers

### Test Data Examples
- Chat model JSON: `{ "outputs": [{ "content": [{"type":"output_text","text":"Hello from LLM"}]}] }` (example)
- Reasoning JSON: `{ "metadata": { "thoughts": "I think..." }, "output": "Final text" }`

### Success Criteria
- All behavior requirements covered by tests
- Graceful handling of errors and timeouts
- Headers present and API key not logged
