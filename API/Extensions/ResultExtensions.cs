using FoodOrderAPI.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderAPI.API.Extensions;

public static class ResultExtensions
{
    public static ActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new OkResult();

        if (result.IsNotFound)
            return new NotFoundObjectResult(new { error = result.Error });

        return new BadRequestObjectResult(new { error = result.Error });
    }

    public static ActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        if (result.IsNotFound)
            return new NotFoundObjectResult(new { error = result.Error });

        return new BadRequestObjectResult(new { error = result.Error });
    }

    public static ActionResult ToCreatedAtActionResult<T>(
        this Result<T> result,
        ControllerBase controller,
        string actionName,
        Func<T, object> routeValues)
    {
        if (result.IsSuccess)
            return controller.CreatedAtAction(actionName, routeValues(result.Value), result.Value);

        if (result.IsNotFound)
            return new NotFoundObjectResult(new { error = result.Error });

        return new BadRequestObjectResult(new { error = result.Error });
    }
}
