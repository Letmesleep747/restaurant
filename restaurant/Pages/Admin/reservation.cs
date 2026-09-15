using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using restaurant.Data;
using restaurant.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

[Authorize(Roles = "Staff,Administrator")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public List<Reservation> Reservations { get; set; }
    public IndexModel(ApplicationDbContext db) => _db = db;

    public async Task OnGetAsync()
    {
        Reservations = await _db.Reservations
            .Include(r => r.User)
            .Include(r => r.Table)
            .ToListAsync();
    }
}