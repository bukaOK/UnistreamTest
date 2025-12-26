using Microsoft.AspNetCore.Mvc;
using System.Net;
using UnistreamTest.Models.Common;

namespace UnistreamTest.Extensions
{
    public static class ResultsExtensions
    {
        public static IResult MapToHttpResult<TData>(this AppResult<TData> appResult)
        {
            if (!appResult.Success)
            {
                if(appResult.Error == null)
                {
                    throw new NullReferenceException("appResult.Error");
                }

                var problemDetails = new ProblemDetails();
                switch (appResult.Error.Code)
                {
                    case ECommonErrorReasons.Validation:
                        problemDetails.Status = (int)HttpStatusCode.BadRequest;
                        problemDetails.Title = "Ошибка валидации";
                        problemDetails.Detail = appResult.Error.Message;
                        if (appResult.Error is FieldValidationErrorResultInfo fieldValidationErrorResultInfo)
                        {
                            problemDetails.Extensions["errors"] = fieldValidationErrorResultInfo.Errors;
                        }
                        break;
                    case ECommonErrorReasons.NotFound:
                        problemDetails.Status = (int)HttpStatusCode.NotFound;
                        problemDetails.Title = "Ресурс не найден";
                        problemDetails.Detail = appResult.Error.Message;
                        break;
                    default:
                        problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                        problemDetails.Title = "Внутренняя ошибка";
                        problemDetails.Detail = appResult.Error.Message;
                        break;
                }
                return Results.Problem(problemDetails);
            }

            return Results.Ok(appResult.Data);
        }
    }
}
