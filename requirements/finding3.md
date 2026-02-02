Finding #3 — Data Layer Smells

Summary:
- Added typed accessors to reduce unstructured JSON blobs stored in the DB.

Changes:
- `Account.ThemePreference` (NotMapped) — deserializes/serializes `ThemePreferenceJson` using `System.Text.Json`.
- `Story.MetadataMap` (NotMapped) — deserializes/serializes `Metadata` as `Dictionary<string, JsonElement>`.

Why:
- Provides a safe, typed surface for code to interact with complex JSON stored in string fields.
- Makes future validation and migrations easier.

Notes:
- Unit and integration tests were run and passed after the changes.
- Consider adding schema validation or a small `StoryMetadata` typed class if metadata shape stabilizes.