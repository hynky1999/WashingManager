# The goal of project is to develop a system for monitoring and managing washing machines.

## Description of system
There will be 3 entities who will be using the system: students, commissionaires, and admin.

Roles:

    - student
      - Able to see the list of washing machines and their status
      - Able to see the statistics of the machines
      - Able to reserve a washing machine for a specific time period only one reservation per student is allowed

    - commissionaire
        - Able to see the list of washing machines and their status
        - Able to see the statistics of the machines
        - Lends a washing machine to student and check off the machine when it is returned

    - admin
        - Unlimited power manages whole system


### Reservations
Each student can reserve a washing machine for a specific time period.
During this period the student have to come to the commissionaire to pick up the washing machine. If he doesn't do that he will receive a  penalty.

If student fails to return a washing machine in time period he will receive a monetary penalty and any student who has reserved the machine after the student will have period changed accordingly.

Students can cancel their reservation. However they have to do it at least N hours before the start of the reservation.

### Reservation suggestions
- Users will be able to create manual reservation from calendar.
- Users can use the system to suggest reservation.

### Reservation info
- Each student can see his ongoing reservations and cancel them in this section. The past reservations will be also displayed here with amount paid.

### Statistics
Various statistics will be gathered and displayed to all users.
- There will be a graph of machines usage plotted over each day and time period(4 hours span).

- Each machine will have its info(manual, status, etc., usage stats) for users to see.

### Price calculations
- Commissionaire will be shown the amount the student have to pay for their borrow. 


### Optional features (don't have to be implemented)
- Each student will have his own wallet where he can deposit money(using card). He can then use the money to pay for his borrows. This means that a payment system have to be implemented.


## Technical details
The system will be developed in C# 10.0 in Blazor server framework using dotnet core 6.0.

### Why Blazor server and not the wasm version ?
#### Advantages:

    1) There is no need for offline support
    2) Slow first loading time as all dlls have to be downloaded and the app itself too
    3) The app will not be very interactive so the signalR will not be overwhelmed
    4) Easier access to DB, if we used wasm the DB would have to be RESTified and non-standard c# auth would have to be used.
### Disadvantages:
    1) Higher requirements for the server(especially the memory usage), which almost swayed me to use wasm since we will have to make the hosting as cheap as possible. But since heroku will be very likely chosen as hosting service it is better to use Blazor server as heroku doesn't provide free RESTified postresql.

- Postressql will be used as the DB. We need sql DB in order to achieve hard consistency which the nosql word usually don't provide. The scaling is not so import since the system don't expect more than 50 concurrent user.

- Entity framework core will be used for the db connection.

### *Advanced* c# concepts used:
- Linq for DB queries
- Async/Await