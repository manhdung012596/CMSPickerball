using System.Collections.Generic;
using System.Threading.Tasks;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Services;

public interface ICustomerService
{
    Task<List<CustomerListDto>> GetCustomersAsync();
    Task<UpdateCustomerDto?> GetCustomerByIdForEditAsync(int id);
    Task<bool> CreateCustomerAsync(CreateCustomerDto customerDto);
    Task<bool> UpdateCustomerAsync(UpdateCustomerDto customerDto);
    Task<bool> DeleteCustomerAsync(int id);
    Task<List<Booking>> GetCustomerBookingsAsync(int customerId);
}
