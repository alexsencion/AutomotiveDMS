using AutoMapper;
using AutomotiveDMS.Application.DTOs.Financing;
using AutomotiveDMS.Domain.Entities;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.Mappings
{
    public class FinancingProfile : Profile
    {
        public FinancingProfile()
        {
            CreateMap<CreateContractDto, FinancingContract>()
                .ForMember(dest => dest.ContractNumber,
                    opt => opt.MapFrom(_ => GenerationContractNumber()))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(_ => ContractStatus.Active))
                .ForMember(dest => dest.EndDate,
                    opt => opt.MapFrom(src =>
                        src.StartDate.AddMonths(src.TermMonths)))
                .ForMember(dest => dest.CreatedDate,
                    opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.FinancedAmount, opt => opt.Ignore())
                .ForMember(dest => dest.MonthlyPayment, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicle, opt => opt.Ignore())
                .ForMember(dest => dest.Guarantors, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentSchedules, opt => opt.Ignore())
                .ForMember(dest => dest.Payments, opt => opt.Ignore())
                .ForMember(dest => dest.PromissoryNotes, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore());

            CreateMap<FinancingContract, ContractDetailDto>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.TotalPaid, opt => opt.Ignore())
                .ForMember(dest => dest.RemainingBalance, opt => opt.Ignore());

            CreateMap<FinancingContract, ContractSummaryDto>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.VehicleDisplay, 
                    opt => opt.MapFrom(src => src.Vehicle != null
                        ? $" {src.Vehicle.Year} {src.Vehicle.Make} {src.Vehicle.Model}"
                        : string.Empty))
                .ForMember(dest => dest.RemainingBalance, opt => opt.Ignore());

            CreateMap<CreateGuarantorDto, Guarantor>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Contract, opt => opt.Ignore());

            CreateMap<Guarantor, GuarantorDto>();

            CreateMap<CreatePromissoryNoteDto, PromissoryNote>()
                .ForMember(dest => dest.NoteNumber,
                    opt => opt.MapFrom(_ => GenerateNoteNumber()))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(_ => PromissoryNoteStatus.Draft))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Contract, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore());

            CreateMap<PromissoryNote, PromissoryNoteDto>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.HasSignedCopy, opt => opt.Ignore());
        }

        private static string GenerationContractNumber() =>
            $"CT-{DateTime.UtcNow:yyyyMM}-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";

        private static string GenerateNoteNumber() =>
            $"PN-{DateTime.UtcNow:yyyyMM}-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";


    }
}
