using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using UPlant.Controllers;
using UPlant.Extensions;
using UPlant.Models;
using UPlant.Models.DB;
using UPlant.Models.ViewModels;
using UPlant.Filters;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using System.Reflection;

CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("it-IT");
CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
var passed = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException(name);
    Console.WriteLine("PASS " + name);
    passed++;
}

using (var db = new CheckContext())
{
    var roles = new[] { "Administrator", "Operator", "Reader", "Discover", "TreeManager" }
        .ToDictionary(name => name, name => new Roles { Id = Guid.NewGuid(), Descr = name });
    ((QuerySet<Roles>)db.Roles).Rows.AddRange(roles.Values);
    var controller = new UsersController(db, null);
    var normalize = typeof(UsersController).GetMethod("NormalizeRoleSelectionAsync", BindingFlags.NonPublic | BindingFlags.Instance);
    foreach (var selected in new[] { new[] { "Reader" }, new[] { "Reader", "Discover" }, new[] { "Reader", "TreeManager" }, new[] { "Reader", "Discover", "TreeManager" }, new[] { "Operator", "Discover", "TreeManager" }, new[] { "Administrator", "Reader", "Discover" }, new[] { "Discover" } })
    {
        var ids = selected.Select(name => roles[name].Id).ToArray();
        var actual = await (Task<List<Guid>>)normalize.Invoke(controller, new object[] { ids });
        var expected = selected.Contains("Administrator") ? new[] { roles["Administrator"].Id } :
            selected.Any(name => name == "Reader" || name == "Operator") ? ids : Array.Empty<Guid>();
        Check(actual.ToHashSet().SetEquals(expected), "User role selection preserves explicit support tools: " + string.Join("+", selected));
    }
}

foreach (var module in new[]
{
    (Controller: typeof(U_Plant.Controllers.PercorsiController), Role: "Discover", OperatorRead: false),
    (Controller: typeof(InterventiAlberiController), Role: "TreeManager", OperatorRead: true),
    (Controller: typeof(IndividuiPercorsoController), Role: "Discover", OperatorRead: false)
})
{
    var filter = module.Controller.GetCustomAttribute<SupportToolAccessAttribute>();
    foreach (var roles in new[] { new[] { "Reader" }, new[] { "Reader", module.Role }, new[] { "Operator" }, new[] { "Operator", module.Role }, new[] { module.Role }, new[] { "Administrator" } })
    foreach (var action in new[] { "Index", "Details", "Create", "Edit", "Delete", "ShowHidden", "Inserisciindividuopercorso", "UploadImg", "DeleteImgConfirmed" })
    {
        var http = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(roles.Select(r => new Claim(ClaimTypes.Role, r)), "test")) };
        var descriptor = new ActionDescriptor { RouteValues = new Dictionary<string, string> { ["action"] = action } };
        var context = new ActionExecutingContext(new ActionContext(http, new RouteData(), descriptor), new List<IFilterMetadata>(), new Dictionary<string, object>(), new object());
        filter.OnActionExecuting(context);
        var read = action == "Index" || action == "Details";
        var expected = roles.Contains("Administrator") ||
            (roles.Contains("Operator") && (roles.Contains(module.Role) || (read && module.OperatorRead))) ||
            (roles.Contains("Reader") && roles.Contains(module.Role) && read);
        Check((context.Result == null) == expected, module.Controller.Name + " " + string.Join("+", roles) + " " + action);
    }
}

foreach (var culture in new[] { "it-IT", "en-US" })
{
    CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
    var row = new RisultatoRicercaInd { datainserimento = new DateTime(2026, 4, 3, 14, 30, 0) };
    var expectedDate = culture == "it-IT" ? "03/04/2026" : "4/3/2026";
    Check(DateLocalizationExtensions.LocalizedDate((Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper)null, row.datainserimento) == expectedDate,
        "Individual search preserves day and month when formatting last modification (" + culture + ")");
    Check(row.datainserimento?.ToString("yyyyMMdd", CultureInfo.InvariantCulture) == "20260403",
        "Individual search date sorting is culture independent (" + culture + ")");
    Check(DateLocalizationExtensions.LocalizedDate((Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper)null, (DateTime?)null) == string.Empty,
        "Missing individual search date remains empty (" + culture + ")");
}
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("it-IT");

foreach (var culture in new[] { "it-IT", "en-US" })
{
    CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
    using var fixture = new Fixture();
    var controller = fixture.Individual();
    var result = controller.Create(Fixture.AccessionId, "nursery") as ViewResult;
    Check(result?.Model is StoricoIndividuo { individuoNavigation: not null } &&
        controller.ViewData["accessione"] is Guid id && id == Fixture.AccessionId,
        "Individual GET returns the view model and accession context (" + culture + ")");
}
CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("it-IT");

foreach (var failure in new[] { "date", "coordinates", "collection", "database" })
{
    using var fixture = new Fixture();
    fixture.Db.FailSave = failure == "database";
    var controller = fixture.Individual();
    var result = await fixture.Submit(controller,
        date: failure == "date" ? "not-a-date" : "2026-11-25",
        collection: failure == "collection" ? Guid.NewGuid() : Fixture.CollectionId,
        latitude: failure == "coordinates" ? "999" : "",
        longitude: failure == "coordinates" ? "11" : "");
    Check(result is ViewResult { Model: StoricoIndividuo { individuoNavigation.note: "preserved note" } } &&
        !controller.ModelState.IsValid && controller.ViewData["collezione"] != null &&
        controller.ViewData["tipo"] as string == "nursery",
        "Individual form survives " + failure + " error");
    Check(fixture.Db.SaveCalls == (failure == "database" ? 1 : 0),
        "Individual save count for " + failure);
    if (failure == "database")
        Check(fixture.Db.SavedTypes.Contains(typeof(Individui)) &&
            fixture.Db.SavedTypes.Contains(typeof(StoricoIndividuo)),
            "Both records belong to the same save attempt");
}
using (var fixture = new Fixture())
{
    var result = await fixture.Submit(fixture.Individual());
    Check(result is RedirectToActionResult { ControllerName: "Accessioni" } &&
        fixture.Db.SaveCalls == 1 && fixture.Db.SavedTypes.Length == 2,
        "Successful individual creation uses one save for both records");
}
using (var fixture = new Fixture())
{
    fixture.Db.FailSave = true;
    var controller = fixture.Accession();
    var result = await fixture.SubmitAccession(controller);
    Check(result is ViewResult && !controller.ModelState.IsValid &&
        new[] { "famiglia", "genere", "specie", "fornitore", "nazione", "regione" }
            .All(key => controller.ViewData[key] != null),
        "Accession SQL failure restores all required dropdowns");
}
using (var fixture = new Fixture())
{
    var controller = fixture.Accession();
    controller.ModelState.AddModelError("numeroEsemplari", "Invalid number");
    controller.ModelState.SetModelValue("note", "attempted note", "attempted note");
    var result = await controller.Edit(Fixture.AccessionId, "IPEN", Guid.NewGuid(),
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Fixture.SpeciesId,
        "IT", "99", "999", "locality", "", "", Guid.NewGuid(), 1, Guid.NewGuid(),
        Guid.NewGuid(), false, "", "attempted note", "", "", "", "");
    Check(result is ViewResult && fixture.Db.SaveCalls == 0 &&
        controller.ViewData["specie"] != null &&
        controller.ModelState["note"].AttemptedValue == "attempted note",
        "Invalid accession edit returns the form with attempted values");
}
using (var fixture = new Fixture())
{
    ((QuerySet<Users>)fixture.Db.Users).Rows.Clear();
    var controller = fixture.Attach(new SpecieController(fixture.Db, null, null, null,
        NullLogger<SpecieController>.Instance));
    var result = await controller.ApplyWfoDecision(new ApplyWfoDecisionInput
    {
        SpecieId = Fixture.SpeciesId,
        ReturnToAudit = true
    });
    Check(result is RedirectToActionResult { ActionName: "ReviewWfo" } &&
        fixture.Db.SaveCalls == 0 && controller.TempData.ContainsKey("WfoError"),
        "WFO refuses an unknown user without writing an empty validation ID");
}
using (var fixture = new Fixture())
{
    fixture.Db.FailSave = true;
    var controller = fixture.Attach(new SettoriController(fixture.Db));
    var result = await controller.Edit(Fixture.SectorId, new Settori
    {
        id = Fixture.SectorId, settore = "Sector", organizzazione = Fixture.OrganizationId
    });
    Check(result is ViewResult && !controller.ModelState.IsValid &&
        controller.ViewData["organizzazione"] != null,
        "Sector update handles SQL errors in the form");
}
using (var fixture = new Fixture())
{
    var controller = fixture.Attach(new CollezioniController(fixture.Db));
    var result = await controller.Create(new Collezioni
    {
        collezione = "Collection", organizzazione = Fixture.OrganizationId, settore = Guid.NewGuid()
    });
    Check(result is ViewResult && !controller.ModelState.IsValid && fixture.Db.SaveCalls == 0,
        "Collection creation refuses an unknown sector before saving");
}
Console.WriteLine(passed + " regression checks passed. No database connection was opened.");

sealed class Fixture : IDisposable
{
    public static readonly Guid AccessionId = Guid.NewGuid();
    public static readonly Guid SpeciesId = Guid.NewGuid();
    public static readonly Guid SectorId = Guid.NewGuid();
    public static readonly Guid CollectionId = Guid.NewGuid();
    public static readonly Guid OrganizationId = Guid.NewGuid();
    public CheckContext Db { get; } = new();
    readonly ServiceProvider services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
    LanguageService Language => new(services.GetRequiredService<IStringLocalizerFactory>());

    public Fixture()
    {
        var species = new Specie { id = SpeciesId, nome_scientifico = "Test species" };
        ((QuerySet<Specie>)Db.Specie).Rows.Add(species);
        ((QuerySet<Users>)Db.Users).Rows.Add(new Users
        {
            Id = Guid.NewGuid(), UnipiUserName = "pietro.picconi", IsEnabled = true,
            Organizzazione = OrganizationId
        });
        ((QuerySet<Organizzazioni>)Db.Organizzazioni).Rows.Add(new Organizzazioni { id = OrganizationId });
        ((QuerySet<Accessioni>)Db.Accessioni).Rows.Add(new Accessioni
        {
            id = AccessionId, specie = SpeciesId, specieNavigation = species,
            progressivo = "2026-0001", dataAcquisizione = new DateTime(2026, 11, 25),
            organizzazione = OrganizationId
        });
        ((QuerySet<Settori>)Db.Settori).Rows.Add(new Settori { id = SectorId, organizzazione = OrganizationId });
        ((QuerySet<Collezioni>)Db.Collezioni).Rows.Add(new Collezioni
        {
            id = CollectionId, settore = SectorId, organizzazione = OrganizationId
        });
    }

    public T Attach<T>(T controller) where T : Controller
    {
        var http = new DefaultHttpContext { RequestServices = services };
        http.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("UnipiUserID", "pietro.picconi@muse.it")], "Check"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = http,
            RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
            ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
        };
        controller.Url = new Microsoft.AspNetCore.Mvc.Routing.UrlHelper(controller.ControllerContext);
        controller.TempData = new TempDataDictionary(http, new MemoryTempData());
        return controller;
    }
    public IndividuiController Individual() =>
        Attach(new IndividuiController(Db, Options.Create(new AppSettings()), null, Language));
    public AccessioniController Accession() => Attach(new AccessioniController(Db,
        new ConfigurationBuilder().Build(), Options.Create(new UPlant.Models.Application()),
        Options.Create(new MapSettings()), Options.Create(new AppSettings()), Language));
    public Task<IActionResult> Submit(IndividuiController controller, string date = "2026-11-25",
        Guid? collection = null, string latitude = "", string longitude = "") =>
        controller.Create(AccessionId, Guid.Empty, "", Guid.NewGuid(), date, Guid.NewGuid(),
            SectorId, collection ?? CollectionId, false, "destination", "preserved note",
            Guid.NewGuid(), Guid.NewGuid(), "operation", Guid.NewGuid(), Guid.NewGuid(),
            "", "", latitude, longitude, "nursery");
    public Task<IActionResult> SubmitAccession(AccessioniController controller) =>
        controller.Create("2026-11-25", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Guid.Empty, SpeciesId, false, "",
            "IT", "99", "999", "locality", true, "", "habitat", Guid.NewGuid(), 1,
            Guid.NewGuid(), Guid.NewGuid(), "note", "", "", "", "", "IPEN");
    public void Dispose() { Db.Dispose(); services.Dispose(); }
}

sealed class MemoryTempData : ITempDataProvider
{
    public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
    public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
}
