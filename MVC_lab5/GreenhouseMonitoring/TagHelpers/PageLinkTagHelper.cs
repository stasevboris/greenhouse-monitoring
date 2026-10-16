using GreenhouseMonitoring.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GreenhouseMonitoring.TagHelpers;

public class PageLinkTagHelper : TagHelper
{
    private readonly IUrlHelperFactory _urlHelperFactory;

    public PageLinkTagHelper(IUrlHelperFactory urlHelperFactory)
    {
        _urlHelperFactory = urlHelperFactory;
    }

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public PageViewModel<object> PageModel { get; set; } = null!;

    public string PageAction { get; set; } = null!;

    [HtmlAttributeName(DictionaryAttributePrefix = "page-url-")]
    public Dictionary<string, object> PageUrlValues { get; set; } = new();

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
        output.TagName = "nav";
        output.Attributes.SetAttribute("aria-label", "Постраничная навигация");

        var ul = new TagBuilder("ul");
        ul.AddCssClass("pagination justify-content-center");

        // Previous
        if (PageModel.HasPreviousPage)
        {
            var li = new TagBuilder("li");
            li.AddCssClass("page-item");
            var a = new TagBuilder("a");
            a.AddCssClass("page-link");
            a.Attributes["href"] = GetPageUrl(PageModel.PageNumber - 1, urlHelper);
            a.InnerHtml.AppendHtml("&laquo; Назад");
            li.InnerHtml.AppendHtml(a);
            ul.InnerHtml.AppendHtml(li);
        }

        // Pages
        int startPage = Math.Max(1, PageModel.PageNumber - 2);
        int endPage = Math.Min(PageModel.TotalPages, PageModel.PageNumber + 2);
        for (int i = startPage; i <= endPage; i++)
        {
            var li = new TagBuilder("li");
            li.AddCssClass($"page-item {(i == PageModel.PageNumber ? "active" : "")}");
            var a = new TagBuilder("a");
            a.AddCssClass("page-link");
            a.Attributes["href"] = GetPageUrl(i, urlHelper);
            a.InnerHtml.Append(i.ToString());
            li.InnerHtml.AppendHtml(a);
            ul.InnerHtml.AppendHtml(li);
        }

        // Next
        if (PageModel.HasNextPage)
        {
            var li = new TagBuilder("li");
            li.AddCssClass("page-item");
            var a = new TagBuilder("a");
            a.AddCssClass("page-link");
            a.Attributes["href"] = GetPageUrl(PageModel.PageNumber + 1, urlHelper);
            a.InnerHtml.AppendHtml("Вперёд &raquo;");
            li.InnerHtml.AppendHtml(a);
            ul.InnerHtml.AppendHtml(li);
        }

        output.Content.AppendHtml(ul);
    }

    private string GetPageUrl(int pageNumber, IUrlHelper urlHelper)
    {
        var values = new Dictionary<string, object>(PageUrlValues)
        {
            ["page"] = pageNumber
        };
        return urlHelper.Action(PageAction, values) ?? "#";
    }
}