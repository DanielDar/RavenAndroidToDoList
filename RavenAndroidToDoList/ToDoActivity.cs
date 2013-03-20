using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.Widget;
using Android.OS;
using Raven.Abstractions.Data;
using RavenAndroidToDoList.Adapters;
using RavenAndroidToDoList.Model;
using System;
using Raven.Client;

namespace RavenAndroidToDoList
{
	[Activity(Label = "RavenAndroidToDoList", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait)]
	public class ToDoActivity : RavenActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			InitilizeList();

			//Link to changes from the server 
			Store.Changes().ForAllDocuments().Subscribe(notification =>
				{
					//Ignore id's not of ToDoItem
					if (notification.Id == null || notification.Id.StartsWith("ToDoItems/") == false)
						return;

					var list = FindViewById<ListView>(Resource.Id.ToDoList);
					var items = (List<ToDoItem>) ((ToDoListAdapter) list.Adapter).Items;
					switch (notification.Type)
					{
						case DocumentChangeTypes.Put:
							using (var session = Store.OpenSession())
							{
								var toDoItem = session.Load<ToDoItem>(notification.Id);

								var updated = false;
								for (int i = 0; i < items.Count; i++)
								{
									if (items[i].Id != notification.Id)
										continue;

									items[i] = toDoItem;
									updated = true;
									break;
								}
								if (updated == false)
								{
									if (string.IsNullOrWhiteSpace(toDoItem.ItemName) == false)
									{
										items.Add(toDoItem);
									}
								}
							}
							break;
						case DocumentChangeTypes.Delete:
							items.RemoveAll(x => x.Id == notification.Id);
							break;
					}

					//When updating a list it is important to call to NotifyDataSetChanged() and you must to in on the UI Thread
					RunOnUiThread(() => ((ToDoListAdapter) list.Adapter).Update());
				});

			// Get our button from the layout resource,
			// and attach an event to it
			var removeButton = FindViewById<Button>(Resource.Id.RemoveButton);
			removeButton.Click += delegate { RemoveItems(); };

			var addButton = FindViewById<Button>(Resource.Id.AddButton);
			addButton.Click += delegate
				{
					AddItem();
				};
		}

		private async Task RemoveItems()
		{
			using (var session = Store.OpenAsyncSession())
			{
				var list = FindViewById<ListView>(Resource.Id.ToDoList);
				var items = ((ToDoListAdapter) list.Adapter).Items;
				foreach (var toDoItem in items)
				{
					if (toDoItem.Selected)
					{
						var item = await session.LoadAsync<ToDoItem>(toDoItem.Id);
						session.Delete(item);
					}
				}

				await session.SaveChangesAsync();
			}
		}

		private void InitilizeList()
		{
			using (var session = Store.OpenAsyncSession())
			{
				session.Query<ToDoItem>()
				       .ToListAsync().ContinueWith(task =>
					       {
						       if (task.IsFaulted)
						       {
							       Console.WriteLine(task.Exception);
							       return;
						       }

						       var items = task.Result;
						       RunOnUiThread(() =>
							       {
								       var list = FindViewById<ListView>(Resource.Id.ToDoList);

								       list.Adapter = new ToDoListAdapter(this, items);
							       });

					       });
			}
		}

		private void AddItem()
		{
			var builder = new AlertDialog.Builder(this);
			var view = LayoutInflater.Inflate(Resource.Layout.ModalDialog, null);
			builder.SetView(view);
			AlertDialog alert = builder.Create();
			alert.SetCancelable(true);
			alert.SetButton("OK", async (s, e) =>
				{
					var name = view.FindViewById<EditText>(Resource.Id.Name).Text;
					if (string.IsNullOrWhiteSpace(name))
						return;

					using (var session = Store.OpenAsyncSession())
					{
						await session.StoreAsync(new ToDoItem {ItemName = name});
						await session.SaveChangesAsync();
					}
				});
			alert.Show();
		}
	}
}