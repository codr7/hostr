## domain
Each concept is represented by a separate table in the database.

### users
A user account is needed to log into the system.
When starting with an empty database, the setup process prompts to create a user.
An additional system user (named `hostr`) is automatically created; but since it has an empty password, it can't be used to log in.
Passwords are hashed using pbkdf2 with custom format.

### events
All modifications to the database are logged as events for audits and statistics.
Each event carries all information needed for replay.

### pools
Pools have names and enable tracking total and used capacity in time.

#### calendars
Calendars specify the total and used capacity for pools in time with minute precision.
When capacity is updated, new segments are created and existing segments shortened as needed.

### units
Units are physical objects, e.g hotel rooms or snowboards.
Units have various rules, e.g whether bookings need to be checked in and/or out.
Each unit is a pool, which is used to track its availability in time.

### products
Products are things that can be sold, and therefore specify a sales tax rate.
Each product is a pool, which is used to track its capacity in time.

### price lists
Price lists are used to segment prices.

#### prices
Prices are actual amounts; specified in time per price list, product and pool.

### tax types
Taxes types are different kinds of taxes, e.g VAT.

#### tax rates
Tax rates are actual percentages, specified in time per type.

### bookings
Bookings specify a start- and end date; a pool, optionally of a unit variety; a quantity; an optional price list, product and total price.
Bookings with non zero quantity updates the calendar for the selected pool and specified time period.
Bookings with non zero total price generate charges.

#### charges
Charges are economic transactions, things being sold.
Each charge is tied to a booking and has a timestamp; a product; and a net and tax amount.