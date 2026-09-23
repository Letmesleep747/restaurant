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
    

        public async Task<List<DiningArea>> GetDiningAreasAsync()
        {
            return await _context.DiningAreas
                .OrderBy(d => d.Name)
                .ToListAsync();
        }


        public async Task<DiningArea?> GetDiningAreaByIdAsync(int id)
        {
            return await _context.DiningAreas
                .FirstOrDefaultAsync(
                    d => d.DiningAreaId == id);
        }


        public async Task<List<DiningArea>>
            GetActiveDiningAreasAsync()
        {
            return await _context.DiningAreas
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }


        public async Task<bool>
            IsDiningAreaActiveAsync(int id)
        {
            return await _context.DiningAreas
                .AnyAsync(
                    d => d.DiningAreaId == id &&
                         d.IsActive);
        }


        public async Task<List<DiningArea>>
            GetDiningAreasForTableEditAsync(
                int currentDiningAreaId)
        {
            return await _context.DiningAreas
                .Where(
                    d => d.IsActive ||
                         d.DiningAreaId == currentDiningAreaId)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }


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


        public async Task CreateDiningAreaAsync(
            DiningArea diningArea)
        {
            diningArea.CreatedAt = DateTime.UtcNow;
            diningArea.UpdatedAt = DateTime.UtcNow;

            _context.DiningAreas.Add(diningArea);

            await _context.SaveChangesAsync();
        }


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


        public async Task<bool>
            ToggleDiningAreaStatusAsync(int id)
        {
            var diningArea =
                await GetDiningAreaByIdAsync(id);


            if (diningArea == null)
            {
                return false;
            }


            diningArea.IsActive =
                !diningArea.IsActive;

            diningArea.UpdatedAt =
                DateTime.UtcNow;


            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<List<RestaurantTable>>
            GetRestaurantTablesAsync()
        {
            return await _context.RestaurantTables
                .Include(t => t.DiningArea)
                .OrderBy(t => t.DiningArea!.Name)
                .ThenBy(t => t.TableNumber)
                .ToListAsync();
        }


        public async Task<RestaurantTable?>
            GetRestaurantTableByIdAsync(int id)
        {
            return await _context.RestaurantTables
                .Include(t => t.DiningArea)
                .FirstOrDefaultAsync(
                    t => t.RestaurantTableId == id);
        }


        public async Task<bool>
            TableNumberExistsAsync(
                string tableNumber,
                int? excludeId = null)
        {
            var query =
                _context.RestaurantTables
                    .Where(
                        t => t.TableNumber == tableNumber);


            if (excludeId.HasValue)
            {
                query = query.Where(
                    t => t.RestaurantTableId
                         != excludeId.Value);
            }


            return await query.AnyAsync();
        }


        public async Task CreateRestaurantTableAsync(
            RestaurantTable restaurantTable)
        {
            restaurantTable.CreatedAt =
                DateTime.UtcNow;

            restaurantTable.UpdatedAt =
                DateTime.UtcNow;


            _context.RestaurantTables
                .Add(restaurantTable);


            await _context.SaveChangesAsync();
        }


        public async Task<bool>
            UpdateRestaurantTableAsync(
                int id,
                int diningAreaId,
                string tableNumber,
                int capacity,
                bool isActive)
        {
            var restaurantTable =
                await GetRestaurantTableByIdAsync(id);


            if (restaurantTable == null)
            {
                return false;
            }


            restaurantTable.DiningAreaId =
                diningAreaId;

            restaurantTable.TableNumber =
                tableNumber;

            restaurantTable.Capacity =
                capacity;

            restaurantTable.IsActive =
                isActive;

            restaurantTable.UpdatedAt =
                DateTime.UtcNow;


            await _context.SaveChangesAsync();


            return true;
        }


        public async Task<bool>
            ToggleRestaurantTableStatusAsync(int id)
        {
            var restaurantTable =
                await GetRestaurantTableByIdAsync(id);


            if (restaurantTable == null)
            {
                return false;
            }


            restaurantTable.IsActive =
                !restaurantTable.IsActive;

            restaurantTable.UpdatedAt =
                DateTime.UtcNow;


            await _context.SaveChangesAsync();


            return true;
        }
    }
}