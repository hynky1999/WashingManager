Localization
------------
The app uses LocalizationService to handles localization.
This means:
- strings -> by IStringLocalizer based on the current culture
- dates -> by NodaTime conversion based on the current culture
- models -> by IDBModel description based on the current culture
- currency -> by currencyApproximation based on the current culture

The Current Culture is stored in the cookie. That's why we also use MVC Controller to handle the cookie.


[!NOTE]
All localization related stuff should be handled in front-end. The back-end should be store and use representation based on it's native culture which is stored in DB. This means no Service should inject ILocalization Service!


Language Change
---------------
For language change we use Current Culcure which we save with cookie.


Timezone Change
---------------
There is currently no handler for that, but whole code is written to support it in future.


NodaTime
--------
The whole app uses NodaTime for handling dates and times.
This is because it enforces Types at dates. Otherwise we have just DateTime. What does it represent ? Date ? Date and Time ?
How can I enforce that I want only timezoned date and time ?

Those are problems with DateTime and that's we use NodaTime.

This however creates a bit of obstacle since the default UI Date Components use DateTime and thus respective converters had to be created. 