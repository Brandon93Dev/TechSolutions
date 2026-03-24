using Microsoft.EntityFrameworkCore;
using TechSolutions_IPS_HW.Data;
using TechSolutions_IPS_HW.Models.Customer;
using TechSolutions_IPS_HW.Models.Enums;
using TechSolutions_IPS_HW.Repositories.Interfaces;

namespace TechSolutions_IPS_HW.Repositories;

/// <summary>
/// Implementation of <see cref="ICustomerRepository"/>
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _db;

    public CustomerRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Customer>> GetAllAsync()
    {
        return await _db.Customers.OrderByDescending(c => c.UpdatedAt).ToListAsync();
    }


    //Gets customers by status
    public async Task<List<Customer>> GetByStatusAsync(CustomerStatus status)
    {
        return await _db.Customers
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        return await _db.Customers.FindAsync(id);
    }

    public async Task<int> CountAsync()
    {
        return await _db.Customers.CountAsync();
    }

    public async Task<int> CountByStatusAsync(CustomerStatus status)
    {
        return await _db.Customers.CountAsync(c => c.Status == status);
    }

    public async Task<int> CountByIdsAndStatusAsync(
        IEnumerable<Guid> customerIds, 
        CustomerStatus status)
    {
        var ids = customerIds.Distinct().ToList();
        if (ids.Count == 0) return 0;

        return await _db.Customers.CountAsync(
            c => ids.Contains(c.CustomerId) && 
            c.Status == status);
    }

    public async Task<List<Guid>> GetAllIdsAsync()
    {
        return await _db.Customers
            .AsNoTracking()
            .Select(c => c.CustomerId)
            .ToListAsync();
    }


    /// <summary>
    /// Gets distribution of customers by country
    /// </summary>
    /// <param name="customerIds"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    public async Task<List<(string Country, int Count)>> 
        GetCountryDistributionByIdsAsync(
        IEnumerable<Guid> customerIds, 
        int take)
    {
        var ids = customerIds.Distinct().ToList();
        if (ids.Count == 0) return [];

        var rows = await _db.Customers
            .AsNoTracking()
            .Where(c => ids.Contains(c.CustomerId))
            .GroupBy(c => !string.IsNullOrWhiteSpace(c.AddressCountry)
                ? c.AddressCountry!
                : (!string.IsNullOrWhiteSpace(c.Nationality) ? c.Nationality! : "Unspecified"))
            .Select(g => new { Country = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(take)
            .ToListAsync();

        return rows.Select(x => (x.Country, x.Count)).ToList();
    }

    public async Task<bool> ExistsByIdNumberAsync(string idNumber)
    {
        return await _db.Customers.AnyAsync(c => c.IdNumber == idNumber);
    }

    public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeCustomerId = null)
    {
        var normalizedEmail = email.Trim();

        var query = _db.Customers.Where(c => c.Email == normalizedEmail);

        if (excludeCustomerId.HasValue && excludeCustomerId.Value != Guid.Empty)
            query = query.Where(c => c.CustomerId != excludeCustomerId.Value);

        return await query.AnyAsync();
    }

    public async Task AddAsync(Customer customer)
    {
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Customer customer)
    {
        customer.UpdatedAt = DateTime.UtcNow;
        _db.Customers.Update(customer);
        await _db.SaveChangesAsync();
    }

    //soft deletes a customer
    public async Task DeleteAsync(Guid id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer != null)
        {
            customer.IsDeleted = true;
            customer.DeletedAtUtc = DateTime.UtcNow;
            customer.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
