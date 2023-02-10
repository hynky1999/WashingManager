Average Usage
-------------

Since we wanted for users to be able to see a charts that show:
- Average use of entities per weekday
- Average use of entities per hour
- Average use of entities per hour in specific weekday

We had to create a new Table for that.
While it's possible to get this info from Borrows table,
it would be:
- Extremely slow to query it
- We couldn't find a query that would do all this on DB and a lot of data would have to be transfered to server.


Inspired by NoSQL duplication of data, we created a table that stores
for every weekday and for every hour(0-23) how many times there was a borrow in this hour-weekday. The only problem is that we had to choose
timezone for this and the conversion to different timezones is not trivial. Thus the timezone is UTC+1(Prague) and the conversion is currently not supported.