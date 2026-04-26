# Frodo — Joomla PHP/XML Year String Changes

**Agent:** frodo-year-joomla  
**Role:** Content Builder  
**Task:** Joomla PHP/XML year string changes  
**Status:** ✅ COMPLETED  
**Timestamp:** 2026-04-01T11:40:10Z

## Summary

Frodo implemented year-as-string throughout the Joomla component:

### PHP Layer Changes

**ResultsService.php**
- lookup method: year parameter int → string|null
- getCarversList method: year parameter int → string|null
- Year validation: is_numeric() → preg_match pattern
- File pattern matching updated for alphanumeric years

**HtmlView.php**
- Parameter validation: year parsing changed to string
- Validation now uses preg_match for alphanumeric check

**default.php Template**
- All year inputs: getInt() → getString()
- Year validation consistent with C# layer

### XML Configuration Changes
- carver/carver.xml: type="number" → type="text" for year fields
- carvers/carvers.xml: type="number" → type="text" for year fields

### Helper Methods
- getAvailableYears() regex: updated to match alphanumeric filenames

## Files Modified
- joomla/src/Service/ResultsService.php
- joomla/src/View/HtmlView.php
- joomla/components/com_showcaseresults/site/tmpl/default.php
- joomla/administrator/components/com_showcaseresults/tmpl/carver/carver.xml
- joomla/administrator/components/com_showcaseresults/tmpl/carvers/carvers.xml

## Validation
- Year parameters accept alphanumeric strings
- Existing numeric years continue to work
- Joomla admin interface accepts text year input
- Backward compatibility maintained
