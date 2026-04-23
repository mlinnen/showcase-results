## PR #33 — tester decision

- Treat the public carvers list as a checked-in roster, not a raw registration roster.
- Validate the rule by counting unique `carver_id` values present in prizes/results and comparing that count with `competitors`.
- Keep docs and manual test plans aligned with that behavior so hidden no-result registrations are expected, not treated as regressions.
