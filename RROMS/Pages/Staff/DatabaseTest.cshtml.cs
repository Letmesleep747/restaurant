
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;

namespace RROMS.Pages.Staff
{
    public class DatabaseTestModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public bool IsConnected { get; set; }
        public string ErrorMessage { get; set; } = "";

        public DatabaseTestModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            try
            {
                IsConnected = await _context.Database.CanConnectAsync();

                if (!IsConnected)
                {
                    ErrorMessage =
                        "Connection failed. Check the application logs. " +
                        "CanConnectAsync returned false.";
                }
            }
            catch (Exception ex)
            {
                IsConnected = false;
                ErrorMessage = ex.Message;
            }
        }
    }
}
