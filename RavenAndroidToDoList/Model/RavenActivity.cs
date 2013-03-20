using Android.App;
using Android.OS;
using Raven.Client;
using Raven.Client.Document;

namespace RavenAndroidToDoList.Model
{
	public class RavenActivity : Activity
	{
		public IDocumentStore Store { get; set; }
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			Store = new DocumentStore
				{
					Url = "http://10.0.0.2:8080", // Never use localhost here, this is the ip of the pc not the android device
					DefaultDatabase = "RavenToDoApp"
				}.Initialize();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			Store.Dispose();
		}
	}
}