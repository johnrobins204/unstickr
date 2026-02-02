# StoryFort
An AI-enabled children's writing tutor that keeps creativity in the hands of the child

## CI

This repository includes a minimal GitHub Actions workflow to run build and tests. Add a repository secret named `COHERE_API_KEY` for running integration tests against the real LLM; CI will fall back to a mocked LLM if absent.

Badge (after first run):

![CI](.github/workflows/ci.yml)

## Local commands

Run build and tests locally:

```bash
dotnet restore StoryFort/StoryFort.csproj
dotnet build StoryFort/StoryFort.csproj
dotnet test StoryFort.Tests.Unit/StoryFort.Tests.Unit.csproj
dotnet test StoryFort.Tests.Integration/StoryFort.Tests.Integration.csproj
```
