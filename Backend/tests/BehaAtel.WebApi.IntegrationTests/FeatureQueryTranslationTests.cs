using BehaAtel.Application.Features.Announcements.Common;
using BehaAtel.Application.Features.AuditLogs.Common;
using BehaAtel.Application.Features.Buildings.Common;
using BehaAtel.Application.Features.Charges.Common;
using BehaAtel.Application.Features.Dashboard.Common;
using BehaAtel.Application.Features.Me.Common;
using BehaAtel.Application.Features.Me.GetMyAnnouncements;
using BehaAtel.Application.Features.Payments.Common;
using BehaAtel.Application.Features.Registrations.Common;
using BehaAtel.Application.Features.Residents.Common;
using BehaAtel.Application.Features.Residents.GetResidentById;
using BehaAtel.Application.Features.Rooms.Common;
using BehaAtel.Application.Features.Settings.Common;
using BehaAtel.Application.Features.Admins.Common;
using BehaAtel.Domain.Enums;
using BehaAtel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.WebApi.IntegrationTests;

public sealed class FeatureQueryTranslationTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=query_translation_test;Username=test;Password=test")
            .Options;
        var context = new ApplicationDbContext(options);
        context.Database.SetConnectionString("Host=localhost;Database=query_translation_test;Username=test;Password=test");
        return context;
    }

    [Fact]
    public void ChargesAndResidentPortalQueries_TranslateWithFiltersAggregatesAndSorts()
    {
        using var db = CreateContext();
        var period = new DateOnly(2026, 10, 1);
        var nextPeriod = period.AddMonths(1);
        var charges = db.Charges.AsNoTracking()
            .Where(charge => charge.ResidentId != Guid.Empty)
            .Where(charge => charge.Period == period)
            .Where(charge => charge.Status == ChargeStatus.Unpaid)
            .Where(charge => charge.Type == ChargeType.Rent);
        var chargeQuery =
            from charge in charges
            join resident in db.Residents.AsNoTracking().Where(resident => !resident.IsDeleted)
                on charge.ResidentId equals resident.Id
            join user in db.Users.AsNoTracking().Where(user => !user.IsDeleted)
                on resident.UserId equals user.Id
            let paidAmount = db.PaymentAllocations
                .Where(allocation => allocation.ChargeId == charge.Id)
                .Sum(allocation => (decimal?)allocation.Amount) ?? 0m
            orderby charge.DueDate descending, user.FullName
            select new ChargeDto(
                charge.Id, charge.ResidentId, user.FullName, charge.Period, charge.Type, charge.Amount,
                paidAmount, charge.Amount - paidAmount, charge.DueDate, charge.Status);
        AssertSql(chargeQuery);

        var residentId = Guid.NewGuid();
        var myCharges = db.Charges.AsNoTracking()
            .Where(charge => charge.ResidentId == residentId)
            .Where(charge => charge.Period >= period && charge.Period < nextPeriod)
            .Where(charge => charge.Status != ChargeStatus.Cancelled)
            .OrderByDescending(charge => charge.Period)
            .Select(charge => new
            {
                Charge = charge,
                PaidAmount = db.PaymentAllocations
                    .Where(allocation => allocation.ChargeId == charge.Id)
                    .Sum(allocation => (decimal?)allocation.Amount) ?? 0m
            })
            .Select(item => new MyChargeDto(
                item.Charge.Id, item.Charge.Period, item.Charge.Type, item.Charge.Description,
                item.Charge.Amount, item.PaidAmount, item.Charge.Amount - item.PaidAmount,
                item.Charge.DueDate, item.Charge.Status));
        AssertSql(myCharges);

        var payments = db.Payments.AsNoTracking()
            .Where(payment => payment.ResidentId == residentId)
            .Where(payment => payment.PaidAt >= DateTime.UtcNow.AddMonths(-1))
            .Where(payment => payment.PaidAt <= DateTime.UtcNow)
            .OrderByDescending(payment => payment.PaidAt)
            .Select(payment => new MyPaymentDto(
                payment.Id, payment.Amount, payment.Method, payment.PaidAt, payment.ReceiptNumber, payment.Status));
        AssertSql(payments);

        var currentPeriod = period;
        var mySummary = db.Residents.AsNoTracking()
            .Where(resident => resident.UserId == residentId)
            .Select(resident => new
            {
                Balance = (db.Payments.Where(payment =>
                        payment.ResidentId == resident.Id && payment.Status == PaymentStatus.Completed)
                    .Sum(payment => (decimal?)payment.Amount) ?? 0m)
                    - (db.Charges.Where(charge =>
                        charge.ResidentId == resident.Id && charge.Status != ChargeStatus.Cancelled)
                        .Sum(charge => (decimal?)charge.Amount) ?? 0m),
                Due = db.Charges.Where(charge =>
                        charge.ResidentId == resident.Id &&
                        charge.Period >= currentPeriod &&
                        charge.Period < nextPeriod &&
                        charge.Status != ChargeStatus.Cancelled)
                    .Sum(charge => (decimal?)(charge.Amount -
                        (db.PaymentAllocations.Where(allocation => allocation.ChargeId == charge.Id)
                            .Sum(allocation => (decimal?)allocation.Amount) ?? 0m))) ?? 0m,
                NextDueDate = db.Charges.Where(charge =>
                        charge.ResidentId == resident.Id &&
                        charge.Status != ChargeStatus.Cancelled &&
                        charge.Amount > (db.PaymentAllocations.Where(allocation => allocation.ChargeId == charge.Id)
                            .Sum(allocation => (decimal?)allocation.Amount) ?? 0m))
                    .Min(charge => (DateOnly?)charge.DueDate),
                LastPayment = db.Payments.Where(payment =>
                        payment.ResidentId == resident.Id && payment.Status == PaymentStatus.Completed)
                    .OrderByDescending(payment => payment.PaidAt)
                    .Select(payment => new MyLastPaymentDto(payment.Amount, payment.PaidAt))
                    .FirstOrDefault()
            });
        AssertSql(mySummary);

        var chart = db.Charges.AsNoTracking()
            .Where(charge => charge.ResidentId == residentId &&
                             charge.Status != ChargeStatus.Cancelled &&
                             charge.Period >= period && charge.Period < nextPeriod)
            .GroupBy(charge => new { charge.Period.Year, charge.Period.Month })
            .Select(group => new { group.Key.Year, group.Key.Month, Amount = group.Sum(charge => charge.Amount) });
        AssertSql(chart);

        var paidChart = db.Payments.AsNoTracking()
            .Where(payment => payment.ResidentId == residentId &&
                              payment.Status == PaymentStatus.Completed &&
                              payment.PaidAt >= DateTime.UtcNow.AddMonths(-12) &&
                              payment.PaidAt < DateTime.UtcNow)
            .GroupBy(payment => new { payment.PaidAt.Year, payment.PaidAt.Month })
            .Select(group => new
            {
                group.Key.Year,
                group.Key.Month,
                Amount = group.Sum(payment => payment.Amount)
            });
        AssertSql(paidChart);

        var residentAnnouncements = db.Announcements.AsNoTracking()
            .Where(announcement => !announcement.IsDeleted && announcement.PublishedAt.HasValue)
            .OrderByDescending(announcement => announcement.PublishedAt)
            .Select(announcement => new MyAnnouncementDto(
                announcement.Id,
                announcement.Title,
                announcement.Body,
                announcement.PublishedAt!.Value));
        AssertSql(residentAnnouncements);
    }

    [Fact]
    public void AdminPaymentResidentAndDebtQueries_Translate()
    {
        using var db = CreateContext();
        var payments = db.Payments.AsNoTracking()
            .Where(payment => payment.ResidentId != Guid.Empty)
            .Where(payment => payment.Status == PaymentStatus.Completed)
            .Where(payment => payment.Method == PaymentMethod.Cash)
            .Where(payment => payment.PaidAt >= DateTime.UtcNow.AddDays(-30))
            .Where(payment => payment.Amount >= 1m && payment.Amount <= 10000m);
        var paymentQuery =
            from payment in payments
            join resident in db.Residents.AsNoTracking().Where(resident => !resident.IsDeleted)
                on payment.ResidentId equals resident.Id
            join user in db.Users.AsNoTracking().Where(user => !user.IsDeleted)
                on resident.UserId equals user.Id
            let allocated = db.PaymentAllocations.Where(allocation => allocation.PaymentId == payment.Id)
                .Sum(allocation => (decimal?)allocation.Amount) ?? 0m
            orderby payment.PaidAt descending
            select new PaymentDto(
                payment.Id, payment.ResidentId, user.FullName, payment.Amount, payment.Method, payment.Status,
                payment.PaidAt, payment.ReceiptNumber, payment.Comment, allocated, payment.Amount - allocated);
        AssertSql(paymentQuery);

        var residentQuery = db.Residents.AsNoTracking().Where(resident => !resident.IsDeleted);
        var debtQuery = residentQuery.WithDebt(db.Payments.AsNoTracking(), db.Charges.AsNoTracking());
        var debtors =
            from resident in debtQuery
            join user in db.Users.AsNoTracking().Where(user => !user.IsDeleted && user.Role == Role.User)
                on resident.UserId equals user.Id
            select new
            {
                resident.Id,
                user.FullName,
                Debt = (db.Charges.Where(charge =>
                        charge.ResidentId == resident.Id && charge.Status != ChargeStatus.Cancelled)
                    .Sum(charge => (decimal?)charge.Amount) ?? 0m)
                    - (db.Payments.Where(payment =>
                        payment.ResidentId == resident.Id && payment.Status == PaymentStatus.Completed)
                        .Sum(payment => (decimal?)payment.Amount) ?? 0m),
                OldestDue = db.Charges.Where(charge =>
                        charge.ResidentId == resident.Id &&
                        charge.Status != ChargeStatus.Cancelled &&
                        charge.Amount > (db.PaymentAllocations.Where(allocation => allocation.ChargeId == charge.Id)
                            .Sum(allocation => (decimal?)allocation.Amount) ?? 0m))
                    .Min(charge => (DateOnly?)charge.DueDate)
            };
        AssertSql(debtors
            .Where(debtor => debtor.FullName.ToLower().Contains("resident"))
            .OrderByDescending(debtor => debtor.Debt));

        var filteredResidentEntities = residentQuery.WithNoDebt(db.Payments.AsNoTracking(), db.Charges.AsNoTracking())
            .Where(resident => resident.Status == ResidentStatus.Active)
            .Where(resident => (from occupancy in db.Occupancies
                    where occupancy.ResidentId == resident.Id && occupancy.To == null
                    join room in db.Rooms.Where(room => !room.IsDeleted) on occupancy.RoomId equals room.Id
                    select room.BuildingId).Any(buildingId => buildingId != Guid.Empty))
            .Where(resident => db.Occupancies.Any(occupancy =>
                occupancy.ResidentId == resident.Id && occupancy.To == null && occupancy.RoomId != Guid.Empty));
        var residentList =
            from resident in filteredResidentEntities
            join user in db.Users.AsNoTracking().Where(user => !user.IsDeleted && user.Role == Role.User)
                on resident.UserId equals user.Id
            select new
            {
                Resident = resident,
                User = user,
                RoomNumber = (from occupancy in db.Occupancies
                    where occupancy.ResidentId == resident.Id && occupancy.To == null
                    join room in db.Rooms.Where(room => !room.IsDeleted) on occupancy.RoomId equals room.Id
                    select room.Number).FirstOrDefault()
            };
        var filteredResidents = residentList
            .Where(item => item.User.FullName.ToLower().Contains("resident"))
            .OrderBy(item => item.User.FullName)
            .Select(item => new ResidentListItemDto(
                item.Resident.Id, item.User.FullName, item.User.Email, item.User.Phone, item.Resident.Status,
                null, item.RoomNumber,
                (db.Payments.Where(payment =>
                        payment.ResidentId == item.Resident.Id && payment.Status == PaymentStatus.Completed)
                    .Sum(payment => (decimal?)payment.Amount) ?? 0m)
                - (db.Charges.Where(charge =>
                    charge.ResidentId == item.Resident.Id && charge.Status != ChargeStatus.Cancelled)
                    .Sum(charge => (decimal?)charge.Amount) ?? 0m),
                item.User.IsActive));
        AssertSql(filteredResidents);

        var residentDetails = db.Residents.AsNoTracking()
            .Where(resident => resident.Id != Guid.Empty)
            .Select(resident => new
            {
                resident.Id,
                Balance = (db.Payments.Where(payment =>
                        payment.ResidentId == resident.Id && payment.Status == PaymentStatus.Completed)
                    .Sum(payment => (decimal?)payment.Amount) ?? 0m)
                    - (db.Charges.Where(charge =>
                        charge.ResidentId == resident.Id && charge.Status != ChargeStatus.Cancelled)
                        .Sum(charge => (decimal?)charge.Amount) ?? 0m),
                CurrentRoom = (from occupancy in db.Occupancies
                    where occupancy.ResidentId == resident.Id && occupancy.To == null
                    join room in db.Rooms.Where(room => !room.IsDeleted) on occupancy.RoomId equals room.Id
                    join building in db.Buildings.Where(building => !building.IsDeleted)
                        on room.BuildingId equals building.Id
                    select new CurrentRoomDto(building.Name, room.Number, room.Floor, room.PricePerBed))
                    .FirstOrDefault()
            });
        AssertSql(residentDetails);

        AssertSql(residentQuery.WithBalance(db.Payments.AsNoTracking(), db.Charges.AsNoTracking()));
    }

    [Fact]
    public void DashboardQueries_TranslateAggregatesAndCorrelatedOccupancy()
    {
        using var db = CreateContext();
        var start = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1);
        var thisMonth = new DateOnly(2026, 10, 1);
        var nextMonth = thisMonth.AddMonths(1);

        AssertSql(db.Residents.AsNoTracking()
            .Where(resident => !resident.IsDeleted && resident.Status == ResidentStatus.Active)
            .Select(resident => resident.Id));
        AssertSql(db.Rooms.AsNoTracking().Where(room => !room.IsDeleted).Select(room => room.Id));
        AssertSql(db.Rooms.AsNoTracking().Where(room => !room.IsDeleted)
            .Select(room => room.Capacity));
        AssertSql((from currentOccupancy in db.Occupancies.AsNoTracking().Where(occupancy => occupancy.To == null)
            join resident in db.Residents.AsNoTracking().Where(resident => !resident.IsDeleted)
                on currentOccupancy.ResidentId equals resident.Id
            join room in db.Rooms.AsNoTracking().Where(room => !room.IsDeleted)
                on currentOccupancy.RoomId equals room.Id
            select currentOccupancy.Id));
        AssertSql(db.Payments.AsNoTracking()
            .Where(payment => payment.Status == PaymentStatus.Completed &&
                              payment.PaidAt >= start && payment.PaidAt < end)
            .Select(payment => payment.Amount));
        AssertSql(db.Charges.AsNoTracking()
            .Where(charge => charge.Period >= thisMonth && charge.Period < nextMonth &&
                             charge.Status != ChargeStatus.Cancelled)
            .Select(charge => charge.Amount));

        var debtorsForSummary = db.Residents.AsNoTracking()
            .Where(resident => !resident.IsDeleted)
            .WithDebt(db.Payments.AsNoTracking(), db.Charges.AsNoTracking());
        AssertSql(debtorsForSummary.Select(resident => resident.Id));
        AssertSql(debtorsForSummary.Select(resident =>
            (db.Charges.Where(charge =>
                    charge.ResidentId == resident.Id && charge.Status != ChargeStatus.Cancelled)
                .Sum(charge => (decimal?)charge.Amount) ?? 0m)
            - (db.Payments.Where(payment =>
                payment.ResidentId == resident.Id && payment.Status == PaymentStatus.Completed)
                .Sum(payment => (decimal?)payment.Amount) ?? 0m)));

        var revenue = db.Payments.AsNoTracking()
            .Where(payment => payment.Status == PaymentStatus.Completed && payment.PaidAt >= start && payment.PaidAt < end)
            .GroupBy(payment => new { payment.PaidAt.Year, payment.PaidAt.Month })
            .Select(group => new
            {
                group.Key.Year,
                group.Key.Month,
                Amount = group.Sum(payment => payment.Amount)
            });
        AssertSql(revenue);

        var methods = db.Payments.AsNoTracking()
            .Where(payment => payment.Status == PaymentStatus.Completed && payment.PaidAt >= start && payment.PaidAt < end)
            .GroupBy(payment => payment.Method)
            .Select(group => new
            {
                Method = group.Key,
                Total = group.Sum(payment => payment.Amount),
                Count = group.Count()
            });
        AssertSql(methods);

        var occupancy = db.Buildings.AsNoTracking()
            .Where(building => !building.IsDeleted)
            .Select(building => new
            {
                BuildingId = building.Id,
                BuildingName = building.Name,
                TotalBeds = db.Rooms.Where(room => room.BuildingId == building.Id && !room.IsDeleted)
                    .Sum(room => (int?)room.Capacity) ?? 0,
                OccupiedBeds = (from room in db.Rooms
                 where room.BuildingId == building.Id && !room.IsDeleted
                 from current in db.Occupancies
                 where current.RoomId == room.Id && current.To == null
                 select current.Id).Count()
            })
            .OrderBy(item => item.BuildingName);
        AssertSql(occupancy);

        var topDebtors = db.Residents.AsNoTracking()
            .Where(resident => !resident.IsDeleted)
            .WithDebt(db.Payments.AsNoTracking(), db.Charges.AsNoTracking())
            .OrderByDescending(resident =>
                (db.Charges.Where(charge =>
                    charge.ResidentId == resident.Id && charge.Status != ChargeStatus.Cancelled)
                    .Sum(charge => (decimal?)charge.Amount) ?? 0m)
                - (db.Payments.Where(payment =>
                    payment.ResidentId == resident.Id && payment.Status == PaymentStatus.Completed)
                    .Sum(payment => (decimal?)payment.Amount) ?? 0m))
            .Select(resident => new TopDebtorDto(
                resident.Id,
                db.Users.Where(user => user.Id == resident.UserId).Select(user => user.FullName).FirstOrDefault()!,
                (from occupancy in db.Occupancies
                 where occupancy.ResidentId == resident.Id && occupancy.To == null
                 join room in db.Rooms.Where(room => !room.IsDeleted) on occupancy.RoomId equals room.Id
                 join building in db.Buildings.Where(building => !building.IsDeleted)
                     on room.BuildingId equals building.Id
                 select building.Name).FirstOrDefault(),
                (from occupancy in db.Occupancies
                 where occupancy.ResidentId == resident.Id && occupancy.To == null
                 join room in db.Rooms.Where(room => !room.IsDeleted) on occupancy.RoomId equals room.Id
                 select room.Number).FirstOrDefault(),
                (db.Charges.Where(charge =>
                        charge.ResidentId == resident.Id && charge.Status != ChargeStatus.Cancelled)
                    .Sum(charge => (decimal?)charge.Amount) ?? 0m)
                - (db.Payments.Where(payment =>
                        payment.ResidentId == resident.Id && payment.Status == PaymentStatus.Completed)
                    .Sum(payment => (decimal?)payment.Amount) ?? 0m)));
        AssertSql(topDebtors);

        var recentPayments =
            from payment in db.Payments.Where(payment => payment.Status == PaymentStatus.Completed)
            join resident in db.Residents.Where(resident => !resident.IsDeleted)
                on payment.ResidentId equals resident.Id
            join user in db.Users.Where(user => !user.IsDeleted) on resident.UserId equals user.Id
            orderby payment.PaidAt descending
            select new RecentPaymentDto(
                payment.Id, user.FullName, payment.Amount, payment.Method, payment.PaidAt, payment.ReceiptNumber);
        AssertSql(recentPayments);
    }

    [Fact]
    public void AnnouncementAuditRegistrationAdminAndSettingsQueries_Translate()
    {
        using var db = CreateContext();

        var announcements = db.Announcements.AsNoTracking()
            .Where(announcement => !announcement.IsDeleted && announcement.PublishedAt.HasValue)
            .Where(announcement => announcement.Title.ToLower().Contains("notice"))
            .OrderByDescending(announcement => announcement.CreatedAt)
            .Join(db.Users.AsNoTracking().Where(user => !user.IsDeleted),
                announcement => announcement.AuthorId,
                user => user.Id,
                (announcement, user) => new AnnouncementDto(
                    announcement.Id, announcement.Title, announcement.Body, announcement.PublishedAt,
                    user.FullName, announcement.CreatedAt));
        AssertSql(announcements);
        AssertSql(db.Announcements.AsNoTracking()
            .Where(announcement => !announcement.IsDeleted && !announcement.PublishedAt.HasValue)
            .OrderBy(announcement => announcement.CreatedAt));

        var auditLogs =
            from log in db.AuditLogs.AsNoTracking()
                .Where(log => log.CreatedAt >= DateTime.UtcNow.AddDays(-30) &&
                              log.EntityName == "Resident" &&
                              log.Action == "Update" &&
                              log.UserId != null)
                .OrderByDescending(log => log.CreatedAt)
            join user in db.Users.AsNoTracking().Where(user => !user.IsDeleted)
                on log.UserId equals (Guid?)user.Id into users
            from user in users.DefaultIfEmpty()
            select new AuditLogDto(
                log.Id, log.UserId, user == null ? null : user.FullName, log.Action, log.EntityName,
                log.EntityId, log.OldValues, log.NewValues, log.IpAddress, log.CreatedAt);
        AssertSql(auditLogs);

        var registrationRequests = db.Users.AsNoTracking()
            .Where(user => user.Role == Role.User && user.ApprovalStatus == ApprovalStatus.Pending)
            .Where(user => user.Email.ToLower().Contains("resident"))
            .OrderByDescending(user => user.RegistrationRequestedAt)
            .Select(user => new RegistrationRequestDto(
                user.Id, user.FullName, user.Email, user.Phone, user.RegistrationRequestedAt,
                user.ApprovalStatus, user.EmailConfirmed));
        AssertSql(registrationRequests);

        var admins = db.Users.AsNoTracking()
            .Include(user => user.Permissions)
            .Where(user => user.Role == Role.Admin && user.IsActive)
            .Where(user => user.Email.ToLower().Contains("admin"))
            .OrderBy(user => user.FullName)
            .Select(user => new { user.Id, user.FullName, user.Email, user.CreatedAt, user.LastLoginAt });
        AssertSql(admins);

        var settings = db.Settings.AsNoTracking()
            .Where(setting => SettingKeys.All.Contains(setting.Key))
            .Select(setting => new SettingDto(setting.Key, setting.Value, setting.UpdatedAt));
        AssertSql(settings);

        var adminCountAndPage = db.Users.AsNoTracking()
            .Where(user => user.Role == Role.Admin && user.IsActive)
            .Where(user => user.Email.ToLower().Contains("admin"))
            .OrderByDescending(user => user.LastLoginAt)
            .Skip(0)
            .Take(20);
        AssertSql(adminCountAndPage);
    }

    private static void AssertSql<T>(IQueryable<T> query)
    {
        var sql = query.ToQueryString();
        Assert.False(string.IsNullOrWhiteSpace(sql));
    }
}
