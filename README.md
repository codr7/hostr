## domain concepts
Each concept is represented by a separate table in the database.

### users
A user account is needed to log into the system.<br>
When starting with an empty database, the setup process prompts to create a user.<br>
An additional system user (named `hostr`) is automatically created; but since it has an empty password, it can't be used to log in.<br>
Passwords are hashed using pbkdf2 with custom format.

### events
All modifications to the database are logged as events for audits and statistics.<br>
Each event carries all information needed for replay.

### pools
Pools are things that have a capacity specified in time, even though that capacity may be infinite.<br>
They also contain carry rules, e.g whether bookings need to be checked in and/or out.<br>

#### calendars
Calendars specify the total and used capacity for pools in time with minute precision.<br>
When capacity is updated, new segments are created and existing segments shortened as needed.<br>

### units
Units are physical objects, e.g hotel rooms or snowboards.<br>
Each unit is a pool, which is used to track its availability in time.

### products
Products are things that can be sold.<br>
Each product is a pool, which may be used to track its capacity in time.

### price lists
Price lists are used to segment prices.

#### prices
Prices are actual amounts; specified in time per price list, product and pool.

### tax types
Taxes types are different kinds of taxes, e.g VAT.

#### tax rates
Tax rates are actual percentages, specified in time per type.