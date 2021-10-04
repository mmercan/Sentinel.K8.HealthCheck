using System;
using AutoMapper;

namespace Sentinel.Tests.Helpers
{
    public class GetIMapperExtension
    {
        public static IMapper GetIMapper(Action<IMapperConfigurationExpression> configure)
        {
            var config = new MapperConfiguration(configure);
            //             {
            //                 cfg.AddProfile(new K8SMapper());
            //             });
            var mapper = config.CreateMapper();
            return mapper;
        }

    }
}