IReservation Manager
-------------------
Because we needed to apply penalties for late returns, we needed a way to check if a borrow was not returned on time. 

This is done by having singleton service that performs the checks.
The service has a ITimedQueue for every BorrowableEntity existing.
The when OnChange is invoked on ITimedQueue it will check for that entity if there is either:

- a reservation that still has a borrow and should be ended
- a reservation that ends the soonest

The first one is checked because after restart there could be such a reservation.

The ITimedQueue will then plan a timer that will inform the service when reservation ends. The service will then check if the borrow was return on time and if not it will apply the penalty and resechedule all other reservations.


Source is at <xref:App.Data.ServiceInterfaces.IReservationsService>.