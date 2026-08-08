using AutomotiveDMS.Application.DTOs.Vehicle;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.Validators.Vehicle
{
    public class CreateVehicleDtoValidator : AbstractValidator<CreateVehicleDto>
    {
        public CreateVehicleDtoValidator()
        {
            RuleFor(x => x.Vin)
                .NotEmpty()
                .WithMessage("VIN is required")
                .Length(17)
                .WithMessage("VIN must be exactly 17 characters")
                .Matches(@"[A-HJ-NPR-Z0-9]{17}$")
                .WithMessage("VIN contains invalid characters (no, I, O, Q)");

            RuleFor(x => x.Make)
                .NotEmpty()
                .WithMessage("Make is required")
                .MaximumLength(50)
                .WithMessage("Make cannot exceed 50 characters");

            RuleFor(x => x.Model)
                .NotEmpty()
                .WithMessage("Model is required")
                .MaximumLength(50)
                .WithMessage("Model cannot exceed 50 characters");

            RuleFor(x => x.Color)
                .MaximumLength(50)
                .WithMessage("Trim cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.Color));

            RuleFor(x => x.Trim)
                .MaximumLength(100)
                .WithMessage("Trim cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Trim));

            RuleFor(x => x.PurchasePrice)
                .GreaterThan(0)
                .WithMessage("Purchase price must be greater than zero");

            RuleFor(x => x.ListPrice)
                .GreaterThan(0)
                .WithMessage("List price must be greater than zero");

            RuleFor(x => x.ListPrice)
                .GreaterThanOrEqualTo(x => x.PurchasePrice)
                .WithMessage("List price must be at least equal to purchase price")
                .When(x => x.ListPrice > 0 && x.PurchasePrice > 0);

            RuleFor(x => x.Mileage)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Mileage cannot be negative");

            RuleFor(x => x.EngineType)
                .MaximumLength(50)
                .WithMessage("Engine type cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.EngineType));

            RuleFor(x => x.Transmission)
                .MaximumLength(50)
                .WithMessage("Transmission cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.Transmission));

            RuleFor(x => x.ZoneId)
                .GreaterThan(0)
                .WithMessage("Zone must be selected");

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .WithMessage("Notes cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));


        }
    }
}
