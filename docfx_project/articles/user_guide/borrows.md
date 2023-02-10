Borrows
-------


### Creating a borrow
For a borrow to happen the user first must have reservation.
Once the reservation time comes, the user can borrow the entity.

The borrow is done by Admin or Manager. The users come to manager and ask for the entity. The manager will go to Manage Reservations tab and create a borrow for the user.


### Returning a borrow
The user can return the borrow at any time during the reservation period. If the borrow is not returned on time, the user will be penalized. To return the borrow, the user must come to the manager and return the entity. The manager will then go to Manage Borrows tab and return the borrow.


### Price of a borrow
The price of a borrow is calculated by the following formula:

    price = floor(6 * TotalMinutes/30 + 10)

### Penalty
- No Reservation Pickup Penalty = 50
- Late Return Penalty = 30 * (TotalOverdueMinutes/2)






