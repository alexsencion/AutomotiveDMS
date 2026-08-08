using AutomotiveDMS.Application.Interfaces.Services;
using AutomotiveDMS.Application.Mappings;
using AutomotiveDMS.Application.Services;
using AutomotiveDMS.Application.Validators.Vehicle;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AutomotiveDMS.Application.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services)
        {
            services.AddAutoMapper(
                cfg => { },
                typeof(VehicleProfile)
            );

            services.AddValidatorsFromAssembly(
                typeof(CreateVehicleDtoValidator).Assembly);

            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IFinancingService, FinancingService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IAuditService, AuditService>();

            return services;
        }
    }
}
