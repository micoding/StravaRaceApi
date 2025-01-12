using AutoMapper;
using StravaRaceAPI.Entities;
using StravaRaceAPI.Models;

namespace StravaRaceAPI;

/// <summary>
///     AutoMapper mapping profile.
/// </summary>
public class StravaMappingProfile : Profile
{
    public StravaMappingProfile()
    {
        CreateMap<string, Sex>().ConvertUsing((src, dest) =>
        {
            dest = src.ToLower() == "m" ? Sex.Male : Sex.Female;
            return dest;
        });

        CreateMap<Sex, string>().ConvertUsing((src, dest) =>
        {
            dest = src == Sex.Male ? "m" : "f";
            return dest;
        });

        CreateMap<Result, ResultDTO>();

        CreateMap<User, AthleteDTO>();

        CreateMap<Event, AllEventDTO>();

        CreateMap<Segment, SegmentDTO>();

        CreateMap<AthleteDTO, User>()
            .ForMember(x => x.Gender, m => m.MapFrom(x => x.Sex))
            .ForMember(x => x.ProfilePictureUrl, opt => opt.MapFrom(x => x.PhotoUrl))
            .ReverseMap()
            .ForMember(x => x.Sex, opt => opt.MapFrom(x => x.Gender))
            .ForMember(x => x.PhotoUrl, opt => opt.MapFrom(x => x.ProfilePictureUrl));

        CreateMap<Event, ShowEventDTO>();

        CreateMap<CreateEventDTO, Event>();

        CreateMap<int, DateTime>().ConvertUsing((src, dest) =>
        {
            dest = DateTime.UnixEpoch.AddSeconds(src);
            return dest;
        });

        CreateMap<TokenDTO, Token>()
            .ForMember(x => x.AccessToken, m => m.MapFrom(x => x.AccessToken))
            .ForMember(x => x.RefreshToken, m => m.MapFrom(x => x.RefreshToken))
            .ForMember(x => x.ExpirationOfToken, m => m.MapFrom(x => x.ExpiresAt));
    }
}