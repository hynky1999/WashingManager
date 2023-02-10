using System.Collections.Generic;
using App.Auth.Models;
using App.Data;
using App.Data.Models;
using App.Data.Utils;

namespace EFCoreTests;

public class DBFullFactory : DbFactory
{
    public DBFullFactory(string name) : base(name)
    {
    }


    protected override void FillData(ApplicationDbContext context)
    {
        var wms = CreateWashingMachines();
        var bps = CreateBorrowPersons();
        //var borrows = CreateBorrows(wms);
        var users = new List<ApplicationUser> {new("admin")};
        // Ok for just testing
        context.AddRange(bps);
        context.AddRange(wms);
        context.AddRange(users);
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
            },
            new()
            {
                Location = new Location
                {
                    Building = 'B',
                    DoorNum = 2,
                    Floor = 0
                },
                Manual = new Manual
                {
                    FileName = "xd.pdf",
                },
                Status = Status.Free
            }
        };
        return washingMachines;
    }

    private List<BorrowPerson> CreateBorrowPersons()
    {
        var borrowPersons = new List<BorrowPerson>
        {
            new() {Name = "Hlynka", Surname = "Sirecek"},
            new() {Name = "Pechoun", Surname = "Buh"},
            new() {Name = "Samuel", Surname = "Lehoun"}
        };
        return borrowPersons;
    }
}

public class DBEmpty : DbFactory
{
    public DBEmpty(string name) : base(name)
    {
    }

    protected override void FillData(ApplicationDbContext context)
    {
    }
}