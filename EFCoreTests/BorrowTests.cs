using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Moq;
using NodaTime;
using PrackyASusarny.Data;
using PrackyASusarny.Data.Constants;
using PrackyASusarny.Data.EFCoreServices;
using PrackyASusarny.Data.LocServices;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Middlewares;
using Xunit;

namespace EFCoreTests;

public class _TEST_DB_BORR : DBFullFactory
{
    public _TEST_DB_BORR() : base("test_borrows")
    {
    }
}

public class BorrowTests : IClassFixture<_TEST_DB_BORR>, IDisposable
{
    public BorrowTests(_TEST_DB_BORR factory)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json")
            .Build();
        CurrencyService = new CurrencyService();
        var middleware = new ContextHookMiddleware();
        Loc = new LocalizationService(new Utils.Clock14082022(),
            CurrencyService, Mock.Of<IStringLocalizer<LocalizationService>>(),
            config);
        Rates = new myRates();

        // No need to add hooks as we won't need them
        IUsageService usageService = new UsageService(Loc, new UsageContants());
        IBorrowPersonService bpService = new BorrowPersonService(factory);
        WmService =
            new CrudService<WashingMachine>(factory, middleware);
        BorrowService = new BorrowService(factory, bpService,
            Loc, usageService, Rates);
        Factory = factory;
        BPService = new BorrowPersonService(factory);
    }

    private CurrencyService CurrencyService { get; }

    private IDbContextFactory<ApplicationDbContext> Factory { get; }
    private ILocalizationService Loc { get; }
    private IRates Rates { get; }
    private IBorrowService BorrowService { get; }
    private IBorrowPersonService BPService { get; }
    private CrudService<WashingMachine> WmService { get; }


    public void Dispose()
    {
        using var context = Factory.CreateDbContext();
        var wms = context.WashingMachines;
        context.RemoveRange(wms);
        var res = context.Reservations;
        context.RemoveRange(res);
        context.SaveChanges();
    }

    [Fact]
    public async Task BorrowOfNonFreeMachine()
    {
        var bp = await BPService.GetByIdAsync(1);
        var wm = new WashingMachine()
        {
            BorrowableEntityID = 10,
            LocationID = 1,
            ManualID = 1,
            Status = Status.Broken,
            Manufacturer = "Test",
        };
        var entity = await WmService.CreateAsync(wm);
        var startTime = Loc.Now;

        // Check if raises ArgumentException
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await BorrowService.AddBorrowAsync(new Borrow()
            {
                BorrowPerson = bp!,
                BorrowableEntity = entity,
                Start = startTime,
                End = null
            });
        });
    }


    [Fact]
    public async Task Price()
    {
        var bp = await BPService.GetByIdAsync(1);
        var wm = new WashingMachine()
        {
            BorrowableEntityID = 10,
            LocationID = 1,
            ManualID = 1,
            Status = Status.Free,
            Manufacturer = "Test",
        };
        var entity = await WmService.CreateAsync(wm);
        var startTime = Loc.Now;
        var end = startTime + Duration.FromHours(1);
        var borrow = await BorrowService.AddBorrowAsync(new Borrow()
        {
            BorrowPerson = bp!,
            BorrowableEntity = entity,
            Start = startTime,
            End = end
        });
        var price = await BorrowService.GetPriceAsync(borrow!);
        var expected = Rates.FlatBorrowPrice + Rates.WMpricePerHalfHour * 2;
        Assert.Equal(expected, price.Amount);
    }

    private class myRates : IRates
    {
        public int WMpricePerHalfHour => 1;
        public int FlatBorrowPrice => 10;
        public int WMpricePerOverRes => 15;
        public int WMNoBorrowPenalty => 20;
        public Currency DBCurrency => Currency.CZK;
    }
}