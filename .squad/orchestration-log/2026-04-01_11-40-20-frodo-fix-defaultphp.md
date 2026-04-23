# Frodo — Fix Missed getInt() in default.php

**Agent:** frodo-fix-defaultphp  
**Role:** Content Builder  
**Task:** Fixed missed getInt() in default.php  
**Status:** ✅ COMPLETED  
**Timestamp:** 2026-04-01T11:40:20Z

## Summary

Frodo corrected the missed year parameter parsing in the template layer.

## Changes Made

**File:** joomla/components/com_showcaseresults/site/tmpl/default.php

- Changed remaining year input from getInt() to getString()
- Ensures consistent parameter parsing across template
- Aligns with C# and schema expectations

## Impact

- Year parameter now correctly parsed as string
- Alphanumeric years will pass through template layer
- No more type mismatch errors

## Validation

- Ready for Aragorn re-validation
- Unblocks full test cycle
