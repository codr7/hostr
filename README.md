
## concepts
Every concept is a separate table in the database.

### users
A user account is needed to log into the system.<br>
When starting with an empty database, the setup process prompts to create a user.<br>
An additional system user (named 'hostr') is automatically created; but since it has an empty password, it can't be used to log in.<br>
Passwords are hashed using pbkdf2 with a custom format.

### pools
Pools are things that have a capacity specified in time, even though that capacity may be infinite.<br>
They also contain rules for booknigs, e.g whether bookings need to be checked in and/or out.<br>

### calendars
Calendars specify the total and used capacity for pools in time, down to minutes.<br>
Every time capacity is updated, new segments are created and existing segments shortened.<br>
Each segment carries an optional label for display purposes.

### units
Units are physical objects, e.g hotel room or a pair of skis.<br>
Every unit is a pool, which is used to track it's availability.

### products
Products are things that can be sold.<br>
Every product is a pool, which may be used to track it's capacity.

### prices
Prices are the actual amounts; specified in time per price list, product and pool.

### price lists
Price lists are used to separate prices into segments.

### taxes
Taxes are different kinds of taxes, e.g VAT.

#### rates
Tax rates are the actual percentages specified in time.