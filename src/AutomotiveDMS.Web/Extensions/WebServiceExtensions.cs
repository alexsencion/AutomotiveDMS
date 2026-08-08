namespace AutomotiveDMS.Web.Extensions
{
    public static class WebServiceExtensions
    {
        public static IServiceCollection AddWebServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddControllersWithViews();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;

                options.Cookie.HttpOnly = true;

                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly",
                    policy => policy.RequireRole("Admin"));

                options.AddPolicy("AdminOrManager",
                    policy => policy.RequireRole("Admin", "Manager"));

                options.AddPolicy("CanViewFinancing",
                    policy => policy.RequireRole("Admin", "Manager", "Finance"));

                options.AddPolicy("CanRecordPayments",
                    policy => policy.RequireRole("Admin", "Manager", "Finance"));

                options.AddPolicy("CanManageVehicles",
                    policy => policy.RequireRole("Admin", "Manager", "Sales"));

                options.AddPolicy("CanViewCustomers",
                    policy => policy.RequireRole("Admin", "Manager", "Sales", "Finance"));
            });

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddHttpContextAccessor();

            return services;
        }
    }
}
