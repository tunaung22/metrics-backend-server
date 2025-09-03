namespace Metrics.Web.Common.Mappers;

public interface IMapper<TSource, TDestination>
{
    TDestination Map(TSource source);
    TSource Map(TDestination destination);
}
