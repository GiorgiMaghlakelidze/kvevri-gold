using KvevriGold.Web;
using KvevriGold.Web.Game;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// The browser runs the same engine as the tests and the simulator, client-side, with play money only.
builder.Services.AddSingleton<DemoSession>();

await builder.Build().RunAsync();
