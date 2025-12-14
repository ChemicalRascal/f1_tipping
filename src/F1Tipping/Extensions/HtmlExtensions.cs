using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Scaffolding;
using System.Linq.Expressions;

namespace F1Tipping.Common
{
    public static class HtmlExtensions
    {
        //extension<T>(IHtmlHelper<T> html)
        //{
        //    public IHtmlContent DescriptionFor<TModel, TValue>(Expression<Func<T, TValue>> expression) where TModel : T
        //    {
        //        ArgumentNullException.ThrowIfNull(html);
        //        ArgumentNullException.ThrowIfNull(expression);

        //        var expressionProvider = html.ViewContext?.HttpContext?.RequestServices?.GetService<ModelExpressionProvider>()
        //            ?? new ModelExpressionProvider(html.MetadataProvider);
        //        var modelExpression = expressionProvider.CreateModelExpression(html.ViewData, expression);

        //        return new HtmlString(modelExpression.Metadata.Description);
        //    }
        //}

        //TODO: Turn this into the new extension block syntax.
        // (No way to use type params in this complex stuff?!)
        public static IHtmlContent DescriptionFor<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            ArgumentNullException.ThrowIfNull(html);
            ArgumentNullException.ThrowIfNull(expression);

            var expressionProvider = html.ViewContext?.HttpContext?.RequestServices?.GetService<ModelExpressionProvider>()
                ?? new ModelExpressionProvider(html.MetadataProvider);
            var modelExpression = expressionProvider.CreateModelExpression(html.ViewData, expression);

            return new HtmlString(modelExpression.Metadata.Description);
        }

        extension<T>(T _) where T : Enum
        {
            public static IEnumerable<SelectListItem> ToSelectList()
            {
                var vals = Enum.GetValues(typeof(T)).Cast<T>();

                foreach (var v in vals)
                {
                    yield return new SelectListItem(v.DisplayName(), (v).ToString());
                }
            }
        }
    }
}
