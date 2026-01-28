# ADR-004: Disaster Recovery via Litestream

**Status:** Proposed  
**Date:** 2026-01-24  
**Scope:** Pilot (500 Users)

## Context
The current architecture uses SQLite in a Docker container. While this simplifies deployment, it presents a "Single Point of Failure" risk. If the container volume is corrupted or the host node fails, all student story data is lost. We currently rely on nightly volume snapshots, which implies a Recovery Point Objective (RPO) of up to 24 hours—unacceptable for student coursework.

## Decision
We will integrate **Litestream** as a sidecar process within the StoryFort Docker container.

## Rationale
Litestream performs streaming replication of SQLite WAL (Write-Ahead Log) pages to cloud object storage (S3-compatible) in real-time.

1.  **Low RPO:** Replicates changes typically within seconds, reducing data loss potential to near-zero.
2.  **Architecture Compatibility:** Works natively with SQLite WAL mode (enabled in the Pilot Patch), requiring no code changes to the application logic.
3.  **Cost Effective:** Uses cheap object storage (e.g., OVH Object Storage) instead of expensive managed SQL instances (RDS/Azure SQL).

## Consequences
*   **Positive:** Transforms local SQLite into a resilient, cloud-backed database.
*   **Positive:** Decouples storage reliability from compute reliability.
*   **Negative:** Adds operational complexity to the container entrypoint (must manage `litestream replicate` process).
*   **Negative:** Restore procedure requires downloading the DB from S3 on container start if the volume is empty (higher TTR - Time to Recovery).

## Implementation Strategy
1.  Configure `AppDbContext` to use `PRAGMA journal_mode=WAL;` (Completed in Pilot Patch).
2.  Update `Dockerfile` to install Litestream.
3.  Modify `entrypoint.sh` to run `litestream replicate -exec "dotnet StoryFort.dll"`.

