using AutoMapper;
using AutomotiveDMS.Application.DTOs.Vehicle;
using AutomotiveDMS.Domain.Entities;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.Mappings
{
    public class VehicleProfile : Profile
    {
        public VehicleProfile()
        {
            /*
            CreateMap<CreateVehicleDto, VehicleStatusHistory>()
                .ForMember(dest => dest.VIN,
                    opt => opt.MapFrom(src => src.Vin.ToUpperInvariant().Trim()))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(_ => VehicleStatus.Available))
                .ForMember(dest => dest.IsActive,
                    opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.CreatedDate,
                    opt => opt.MapFrom(_ => DateTime.UtcNow))

                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.StatusHistory, opt => opt.Ignore())
                .ForMember(dest => dest.ZoneHistory, opt => opt.Ignore())
                .ForMember(dest => dest.PriceHistory, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore());


            CreateMap<VehicleStatusHistory, VehicleListDto>()
                .ForMember(dest => dest.ZoneName,
                    opt => opt.MapFrom(src => src.Zone != null
                    ? src.Zone.Name
                    : string.Empty))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<VehicleStatusHistory, VehicleSummaryDto>()
                .ForMember(dest => dest.Display,
                    opt => opt.MapFrom(src =>
                        $"{src.Year} {src.Make} {src.Model} ({src.Vin})"))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<VehicleStatusHistory, VehicleStatusHistoryDto>()
                .ForMember(dest => dest.OldStatus,
                    opt => opt.MapFrom(src => src.OldStatus.ToString()))
                .ForMember(dest => dest.NewStatus,
                    opt => opt.MapFrom(src => src.NewStatus.ToString()));

            CreateMap<VehicleZoneHistory, VehicleZoneHistoryDto>()
                .ForMember(dest => dest.FromZone,
                    opt => opt.MapFrom(src => src.FromZone != null
                        ? src.FromZone.Name : string.Empty))
                .ForMember(dest => dest.ToZone,
                    opt => opt.MapFrom(src => src.ToZone != null
                        ? src.ToZone.Name : string.Empty))
                .ForMember(dest => dest.MovedBy,
                    opt => opt.MapFrom(src => src.ChangedBy))
                .ForMember(dest => dest.MovedBy,
                    opt => opt.MapFrom(src => src.ChangedDate));

            CreateMap<VehiclePriceHistory, VehiclePriceHistoryDto>()
                .ForMember(dest => dest.PriceType,
                    opt => opt.MapFrom(src => src.PriceType.ToString()));

            */

        }
    }
}
