using AutoMapper;
using Electium.Application.Dtos;
using Electium.Application.ViewModels;
using Electium.Domain.Entities;

namespace Electium.Application.Mappings;

public class GeneralProfile : Profile
{
    public GeneralProfile()
    {
        // Entidad <-> DTO
        CreateMap<Citizen, CitizenDto>().ReverseMap();
        CreateMap<Party, PartyDto>().ReverseMap();
        CreateMap<ElectivePosition, ElectivePositionDto>().ReverseMap();
        CreateMap<Election, ElectionDto>().ReverseMap();

        // Candidate se mapea a mano para que AutoMapper no intente "des-aplanar"
        // PartyName -> Party.Name y crear entidades nuevas por accidente.
        CreateMap<Candidate, CandidateDto>()
            .ForMember(d => d.ElectionName, o => o.MapFrom(s => s.Election != null ? s.Election.Name : string.Empty))
            .ForMember(d => d.PartyName, o => o.MapFrom(s => s.Party != null ? s.Party.Name : string.Empty))
            .ForMember(d => d.PositionName, o => o.MapFrom(s => s.ElectivePosition != null ? s.ElectivePosition.Name : string.Empty))
            .ForMember(d => d.CitizenFullName, o => o.MapFrom(s => s.Citizen != null ? s.Citizen.Name + " " + s.Citizen.LastName : string.Empty));

        CreateMap<CandidateDto, Candidate>()
            .ForMember(d => d.Election, o => o.Ignore())
            .ForMember(d => d.Party, o => o.Ignore())
            .ForMember(d => d.ElectivePosition, o => o.Ignore())
            .ForMember(d => d.Citizen, o => o.Ignore())
            .ForMember(d => d.Votes, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore());

        // DTO <-> ViewModel
        CreateMap<CitizenDto, CitizenViewModel>().ReverseMap();
        CreateMap<PartyDto, PartyViewModel>().ReverseMap();
        CreateMap<ElectivePositionDto, ElectivePositionViewModel>().ReverseMap();
        CreateMap<ElectionDto, ElectionViewModel>().ReverseMap();

        CreateMap<CandidateDto, CandidateViewModel>()
            .ForMember(d => d.Elections, o => o.Ignore())
            .ForMember(d => d.Parties, o => o.Ignore())
            .ForMember(d => d.Positions, o => o.Ignore())
            .ForMember(d => d.Citizens, o => o.Ignore())
            .ForMember(d => d.PartyLocked, o => o.Ignore());

        CreateMap<CandidateViewModel, CandidateDto>()
            .ForMember(d => d.ElectionName, o => o.Ignore())
            .ForMember(d => d.PartyName, o => o.Ignore())
            .ForMember(d => d.PositionName, o => o.Ignore())
            .ForMember(d => d.CitizenFullName, o => o.Ignore());
    }
}
