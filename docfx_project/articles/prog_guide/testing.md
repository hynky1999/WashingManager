Testing
-------

Testing is done using XUnit tests.
Testing of DB methods is done again testing DB directly.
There are other options to use namely:
- InMemoryDB
- Mocking of DBContext

We couldn't use InMemoryDB as it doesn't support Postgresql and some of our model columns are Postgresql specific.
Same reason for not using mocking of DBContext.

To allow tests to run in parallel, we use a different DB for each test.


Since the db is needed for testing, we need to run the tests in a docker container.
To run the tests, use the following command:
    `docker-compose -f docker-compose-test.yml up --build`

You can also run the tests while having the postgresql db running.
However make sure that you set the environment variable  `ASPNETCORE_ENVIRONMENT` to `Development` and that the connection string in `appsettings.Development.json` is correct.