using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using PrackyASusarny.Auth.Models;
using PrackyASusarny.Data;
using PrackyASusarny.Data.EFCoreServices;
using PrackyASusarny.Data.Models;

namespace EFCoreTests;

public class DBFullFactory : DbFactory
{
    protected override void FillData(ApplicationDbContext context)
    {
        var wms = CreateWashingMachines();
        var borrows = CreateBorrows(wms);
        var users = new List<ApplicationUser> {new ApplicationUser("admin")};
        var reservations = CreateReservations(wms, users);
        // Ok for just testing
        context.AddRange(wms);
        context.AddRange(borrows);
        context.AddRange(reservations);
        context.AddRange(users);
        var usage = CreateUsages();
        context.AddRange(usage);
    }

    private List<Reservation> CreateReservations(List<WashingMachine> wms,
        List<ApplicationUser> users)
    {
        var reservations = new List<Reservation>()
        {
            new()
            {
                User = users[0],
                BorrowableEntity = wms[0],
                Start =
                    new LocalDateTime(2022, 12, 1, 12, 0).InUtc().ToInstant(),
                End = new LocalDateTime(2022, 12, 1, 14, 0).InUtc().ToInstant(),
            },

            new()
            {
                User = users[0],
                BorrowableEntity = wms[0],
                Start =
                    new LocalDateTime(2021, 5, 1, 16, 0).InUtc().ToInstant(),
                End = new LocalDateTime(2021, 5, 1, 18, 0).InUtc().ToInstant(),
            },
        };
        return reservations;
    }

    private List<BorrowableEntityUsage<WashingMachine>> CreateUsages()
    {
        var usages = Enum.GetValues<IsoDayOfWeek>().Select(x =>
            new BorrowableEntityUsage<WashingMachine>
            {
                DayId = x
            }).ToList();
        usages[(int) IsoDayOfWeek.Tuesday].Hour5Total = 100;
        usages[(int) IsoDayOfWeek.Tuesday].Hour9Total = 800;
        usages[(int) IsoDayOfWeek.Saturday].Hour0Total = 100;
        usages[(int) IsoDayOfWeek.Saturday].Hour9Total = 900;
        usages[(int) IsoDayOfWeek.Saturday].Hour10Total = 200;
        return usages;
    }

    private List<WashingMachine> CreateWashingMachines()
    {
        var washingMachines = new List<WashingMachine>
        {
            new()
            {
                Location = new Location
                {
                    Building = 'A',
                    DoorNum = 1,
                    Floor = 0
                },
                Manual = new Manual
                {
                    FileName = "xd.pdf",
                }
            }
        };
        return washingMachines;
    }

    private List<Borrow> CreateBorrows(List<WashingMachine> washingMachines)
    {
        var dater = new LocalizationService(SystemClock.Instance);
        var borrowPersons = new List<BorrowPerson>
        {
            new() {Name = "Hlynka", Surname = "Sirecek"},
            new() {Name = "Pechoun", Surname = "Buh"},
            new() {Name = "Samuel", Surname = "Lehoun"}
        };
        var borrows = new List<Borrow>
        {
            new()
            {
                BorrowableEntity = washingMachines[0],
                startDate = new LocalDateTime(2020, 1, 1, 1, 0)
                    .InZoneLeniently(dater.TimeZone).ToInstant(),
                BorrowPerson = borrowPersons[0],
                endDate = null, xmin = 0
            },
            new()
            {
                BorrowableEntity = washingMachines[0],
                startDate = new LocalDateTime(2020, 1, 2, 0, 0)
                    .InZoneLeniently(dater.TimeZone).ToInstant(),
                BorrowPerson = borrowPersons[0],
                endDate = null, xmin = 0
            },
            new()
            {
                BorrowableEntity = washingMachines[0],
                startDate = new LocalDateTime(2020, 1, 2, 5, 0)
                    .InZoneLeniently(dater.TimeZone).ToInstant(),
                BorrowPerson = borrowPersons[0],
                endDate = null, xmin = 0
            },
            new()
            {
                BorrowableEntity = washingMachines[0],
                startDate = new LocalDateTime(2020, 1, 2, 5, 3)
                    .InZoneLeniently(dater.TimeZone).ToInstant(),
                BorrowPerson = borrowPersons[0],
                endDate = null, xmin = 0
            },

            new()
            {
                BorrowableEntity = washingMachines[0],
                startDate = new LocalDateTime(2020, 1, 4, 10, 0)
                    .InZoneLeniently(dater.TimeZone).ToInstant(),
                BorrowPerson = borrowPersons[0],
                endDate = null, xmin = 0
            }
        };
        return borrows;
    }
}

public class DBEmpty : DbFactory
{
    protected override void FillData(ApplicationDbContext context)
    {
    }
}