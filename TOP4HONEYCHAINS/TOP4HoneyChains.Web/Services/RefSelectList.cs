using Microsoft.AspNetCore.Mvc.Rendering;

namespace TOP4HoneyChains.Web.Services
{
    // D-51: dropdowns of reference data show the selected language; the option values (ids) do not change
    public static class RefSelectList
    {
        public static SelectList Translated(this SelectList list, HttpContext http, string entity, string field)
        {
            var refText = http.RequestServices.GetRequiredService<RefText>();
            var items = list.Select(i => new { i.Value, Text = refText.T(entity, i.Value, field, i.Text) }).ToList();
            return new SelectList(items, "Value", "Text", list.SelectedValue);
        }
    }
}
