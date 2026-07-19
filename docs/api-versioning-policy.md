# TMS API Versioning Policy

## What counts as a breaking change
The following changes require a new API version:
- Removing a field from a response.
- Renaming a field.
- Changing an HTTP status code.
- Tightening validation rules.
- Changing the default sort order.

## What counts as additive (non-breaking)
The following changes are allowed without creating a new version:
- Adding a new optional field.
- Adding a new endpoint.
- Adding a new optional query parameter.

## Sunset window
When a new API version is released, the previous version will remain supported for at least **6 months**. This allows clients, including rural training centres, enough time to migrate.

## Communication
When a version is deprecated, we will:
- Send `Deprecation`, `Sunset`, and `Link` HTTP headers.
- Record the change in the CHANGELOG.
- Notify all API consumers by email.
- Schedule and announce the v1 shutdown date.

## Skipping Versions
Clients may upgrade directly from **V1 to V3** or any later version. Upgrading through every intermediate version is not required.