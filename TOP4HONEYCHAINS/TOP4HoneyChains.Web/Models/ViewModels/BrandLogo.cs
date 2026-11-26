namespace TOP4HoneyChains.Web.Models.ViewModels
{
    // The project's logo (Views/Shared/_BrandLogo.cshtml): Wide = emblem, wordmark and tagline; otherwise the emblem only. Width in px.
    public record BrandLogo(int Width, bool Wide = true, string Alt = "", string? Class = null);

    // The bee animations (Views/Shared/_Bee.cshtml): Kind "loading" (3 s loop) or "landing" (plays once, 3.2 s); the static bee
    // instead when "Show animations" is off (html[data-scenes=off]) or the system asks for reduced motion.
    public record Bee(string Kind, int Width, string Alt = "", string? Class = null);
}
