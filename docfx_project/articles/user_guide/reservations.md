Reservations
------------


### Creating a reservation
In order for user of the app to make a borrow he first must create a reservation. 
This can be done by anyone who is logged in.
To create a reservation, the user can either get a suggestion for a reservation or create a reservation for certain entity in certain time.
This can be done under Create Reservation in the navigation bar.


### Canceling a reservation
In order to cancel a reservation, the user must be logged in and the reservation must be his own. There also must be a minimum of x minutes before the reservation starts. This can be done from My Account -> My Reservations.


### Overdue reservations
When a reservation is overdue the user will be penalized. Then his reservation will be prolonged by specified amount of time. Other reservations that would intersect with this will be postponed.

### No reservation pickup penalty
When a user does not pick up his reservation, he will be penalized.

### Constants

The following constants are used in the app:
- Maximum Reservation Time - 4 days
- Minimum Reservation Time - 1 minute
- Minimum Reservation Time Before Start - 0 minutes
- Minumum Reservation Time Before Start for Res. Cancel - 0 minutes
- Overdue postpone time - 2 minutes
- Max number of reservations - 4
