// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using StreamerbotStreamlootsBridge;
using Websocket.Client;

await Start();

static async Task Start()
{
	Console.WriteLine("Welcome to the Streamer.bot Streamloots Bridge Setup. What do you want to do?");
	Console.WriteLine("\t\"auth\": Setup your authorization token to connect to Streamloots.");
	Console.WriteLine("\t\"pack\": Set the pack that will be gifted. (AUTHORIZATION SETUP NEEDED FIRST!)");
	Console.WriteLine("\t\"exit\": Exit the program.");
	Console.WriteLine("");
	string? input = Console.ReadLine();
	switch (input){
		case "auth":
			AuthSetup();
			await Start();
			break;
		case "pack":
			await PackSetupAsync();
			await Start();
			break;
		case "exit":
			break;
		default:
			Console.WriteLine("I didn't quite get that. Try again! (check spelling)");
			await Start();
			break;
	}
}

static void AuthSetup()
{
	Settings settings = SettingsSaveLoader.ReadSettings();

	Console.WriteLine("Go to the streamloots homepage, make sure you are logged in, then Open the console (google how to do that) and paste the following command in there and press enter.");
	Console.WriteLine("");
	Console.WriteLine("decodeURIComponent(document.cookie).split('; ').forEach(val => { if (val.indexOf(\"AUTH_TOKEN=\") === 0) console.log(val.substring(\"AUTH_TOKEN=\".length));});");
	Console.WriteLine("");
	Console.WriteLine("Paste the result here:");

	string? token = Console.ReadLine();
	if(token != null)
		settings.authToken = token;

	SettingsSaveLoader.SaveSettings(settings);
}

static async Task PackSetupAsync()
{
	Settings settings = SettingsSaveLoader.ReadSettings();
	HttpClient req = new HttpClient();
	Uri sluri = new Uri("https://api.streamloots.com");
	req.DefaultRequestHeaders.Add("Authorization", "Bearer " + settings.authToken);

	Console.WriteLine("What Channel do you want to gift packs from? Enter it's name:");
	string? name = Console.ReadLine();

	var exitEvent = new ManualResetEvent(false);
	Uri uri = new Uri("ws://127.0.0.1:8080/");

	WebsocketClient botclient = new WebsocketClient(uri);

	HttpResponseMessage reply = await req.GetAsync(new Uri("https://api.streamloots.com/sets?slug=" + name));
	string replyString = await reply.Content.ReadAsStringAsync();
	StreamlootsReply? packsObj = JsonConvert.DeserializeObject<StreamlootsReply>(replyString);

	int cnt = 0;
	if(packsObj != null)
	{
		foreach (CardPack pack in packsObj.data)
		{
			cnt++;
			Console.WriteLine(cnt + ": " + pack.name);
		}

		Console.WriteLine("\nPlease enter the index of the pack you want to be gifted: ");
		int ind = Convert.ToInt32(Console.ReadLine());

		settings.packId = packsObj.data[ind]._id;
	}

	SettingsSaveLoader.SaveSettings(settings);
}