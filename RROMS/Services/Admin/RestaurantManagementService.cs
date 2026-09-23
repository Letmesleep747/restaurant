using Microsoft.EntityFrameworkCore;
using RROMS.Data;
using RROMS.Models;

namespace RROMS.Services.Admin
{
    public class RestaurantManagementService
    {
        private readonly ApplicationDbContext _context;

        public RestaurantManagementService(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // Get all dining areas
        public async Task<List<DiningArea>> GetDiningAreasAsync()
        {
            return await _context.DiningAreas
                .OrderBy(d => d.Name)
                .ToListAsync();
        }


        // Get one dining area
        public async Task<DiningArea?> GetDiningAreaByIdAsync(int id)
        {
            return await _context.DiningAreas
                .FirstOrDefaultAsync(d => d.DiningAreaId == id);
        }


        // Check duplicate name
        public async Task<bool> DiningAreaNameExistsAsync(
            string name,
            int? excludeId = null)
        {
            var query = _context.DiningAreas
                .Where(d => d.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(
                    d => d.DiningAreaId != excludeId.Value);
            }

            return await query.AnyAsync();
        }


        // Create
        public async Task CreateDiningAreaAsync(
            DiningArea diningArea)
        {
            diningArea.CreatedAt = DateTime.UtcNow;
            diningArea.UpdatedAt = DateTime.UtcNow;

            _context.DiningAreas.Add(diningArea);

            await _context.SaveChangesAsync();
        }


        // Update
        public async Task<bool> UpdateDiningAreaAsync(
            int id,
            string name,
            string? description,
            bool isActive)
        {
            var diningArea =
                await GetDiningAreaByIdAsync(id);

            if (diningArea == null)
            {
                return false;
            }

            diningArea.Name = name;
            diningArea.Description = description;
            diningArea.IsActive = isActive;
            diningArea.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }


        // Activate / deactivate
        public async Task<bool> ToggleDiningAreaStatusAsync(int id)
        {
            var diningArea =
                await GetDiningAreaByIdAsync(id);

            if (diningArea == null)
            {
                return false;
            }

            diningArea.IsActive = !diningArea.IsActive;
            diningArea.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}