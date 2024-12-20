using AutoMapper;
using StravaRaceAPI.Entities;
using StravaRaceAPI.Models;

namespace StravaRaceAPI;

public class StravaMappingProfile : Profile
{
    public StravaMappingProfile()
    {
        CreateMap<string, Sex>().ConvertUsing((src, dest) =>
        {
            dest = src.ToLower() == "m" ? Sex.Male : Sex.Female;

            return dest;
        });

        CreateMap<AthleteDTO, User>()
            .ForMember(x => x.Gender, m => m.MapFrom(x => x.Sex));

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