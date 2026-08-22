using AutomotiveDMS.Application.DTOs.Customer;
using AutomotiveDMS.Domain.Entities;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.Mappings
{
    public class CustomerProfile : VehicleProfile
    {
        public CustomerProfile()
        {
            CreateMap<CreateCustomerDto, Customer>()
                .ForMember(dest => dest.CustomerType,
                    opt => opt.MapFrom(src =>
                        Enum.Parse<CustomerType>(src.CustomerType, ignoreCase: true)))
                .ForMember(dest => dest.IsActive,
                    opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.CreatedDate,
                    opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Contracts, opt => opt.Ignore())
                .ForMember(dest => dest.CommunicationLogs, opt => opt.Ignore())
                .ForMember(dest => dest.InteractionNotes, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore());

            CreateMap<Customer, CustomerListDto>()
                .ForMember(dest => dest.DisplayName,
                    opt => opt.MapFrom(src => BuildDisplayName(src)))
                .ForMember(dest => dest.CustomerType,
                    opt => opt.MapFrom(src => src.CustomerType.ToString()))
                .ForMember(dest => dest.IdNumber,
                    opt => opt.MapFrom(src =>
                        src.CustomerType == CustomerType.Individual
                            ? src.Cedula
                            : src.Rnc))
                .ForMember(dest => dest.ActiveContracts, opt => opt.Ignore());

            CreateMap<Customer, CustomerDetailDto>()
                .ForMember(dest => dest.DisplayName,
                    opt => opt.MapFrom(src => BuildDisplayName(src)))
                .ForMember(dest => dest.CustomerType,
                    opt => opt.MapFrom(src => src.CustomerType.ToString()));

            CreateMap<Customer, CustomerSummaryDto>()
                .ForMember(dest => dest.DisplayName,
                    opt => opt.MapFrom(src => BuildDisplayName(src)))
                .ForMember(dest => dest.IdNumber,
                    opt => opt.MapFrom(src =>
                        src.CustomerType == CustomerType.Individual
                            ? src.Cedula ?? string.Empty
                            : src.Rnc ?? string.Empty));
        }

        private static string BuildDisplayName(Customer customer) =>
            customer.CustomerType == CustomerType.Individual
                ? $"{customer.FirstName} {customer.LastName}".Trim()
                : customer.CompanyName ?? string.Empty;
    }
}
