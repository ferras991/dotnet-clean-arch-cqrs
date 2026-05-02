using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Api.Conventions;

public sealed class GlobalRoutePrefixConvention(string prefix) : IApplicationModelConvention
{
    private readonly AttributeRouteModel _prefixRoute = new(new RouteAttribute(prefix));

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        foreach (var selector in controller.Selectors)
            selector.AttributeRouteModel = selector.AttributeRouteModel is not null
                ? AttributeRouteModel.CombineAttributeRouteModel(_prefixRoute, selector.AttributeRouteModel)
                : new AttributeRouteModel(_prefixRoute);
    }
}