using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using SoccerPitchMvc.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoccerPitchMvc.Services;

public class CustomerService : ICustomerService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IMapper _mapper;

    public CustomerService(IDbContextFactory<ApplicationDbContext> dbFactory, IMapper mapper)
    {
        _dbFactory = dbFactory;
        _mapper = mapper;
    }

    public async Task<List<CustomerListDto>> GetCustomersAsync()
    {
        using var context = _dbFactory.CreateDbContext();
        var customers = await context.Customers
            .OrderBy(c => c.FullName)
            .ToListAsync();
        return _mapper.Map<List<CustomerListDto>>(customers);
    }

    public async Task<UpdateCustomerDto?> GetCustomerByIdForEditAsync(int id)
    {
        using var context = _dbFactory.CreateDbContext();
        var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null) return null;
        return _mapper.Map<UpdateCustomerDto>(customer);
    }

    public async Task<bool> CreateCustomerAsync(CreateCustomerDto customerDto)
    {
        using var context = _dbFactory.CreateDbContext();
        bool phoneExists = await context.Customers.AnyAsync(c => c.PhoneNumber == customerDto.PhoneNumber);
        if (phoneExists) return false;

        var customer = _mapper.Map<Customer>(customerDto);
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateCustomerAsync(UpdateCustomerDto customerDto)
    {
        using var context = _dbFactory.CreateDbContext();
        bool phoneExists = await context.Customers.AnyAsync(c => c.PhoneNumber == customerDto.PhoneNumber && c.Id != customerDto.Id);
        if (phoneExists) return false;

        var dbCustomer = await context.Customers.FindAsync(customerDto.Id);
        if (dbCustomer == null) return false;

        _mapper.Map(customerDto, dbCustomer);
        context.Customers.Update(dbCustomer);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        using var context = _dbFactory.CreateDbContext();
        var dbCustomer = await context.Customers.FindAsync(id);
        if (dbCustomer == null) return false;

        context.Customers.Remove(dbCustomer);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Booking>> GetCustomerBookingsAsync(int customerId)
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Bookings
            .Include(b => b.Pitch)
            .Include(b => b.TimeSlot)
            .Where(b => b.CustomerId == customerId)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    }
}
