using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Views;
using Android.Widget;
using RavenAndroidToDoList.Model;

namespace RavenAndroidToDoList.Adapters
{
	public class ToDoListAdapter : BaseAdapter<ToDoItem>
	{
		private readonly Activity context;
		public IList<ToDoItem> Items { get; set; }

		public ToDoListAdapter(Activity context, IList<ToDoItem> items)
		{
			this.context = context;
			Items = items;
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public void Update()
		{
			//Important to be called on the UI Thread
			NotifyDataSetChanged();
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var item = Items[position];

			var view = convertView;

			if (convertView == null || !(convertView is LinearLayout))
				view = context.LayoutInflater.Inflate(Resource.Layout.ToDoItemLayout, parent, false);

			//Find references to each subview in the list item's view
			var checkBox = view.FindViewById(Resource.Id.checkBox1) as CheckBox;

			//Assign this item's values to the various subviews
			checkBox.Checked = item.Selected;
			checkBox.Text = item.ItemName;
			checkBox.Click += async (sender, args) =>
			{
				var box = ((CheckBox)sender);
				var name = box.Text;
				using (var session = ((ToDoActivity)context).Store.OpenAsyncSession())
				{
					//update the server for selecting item
					foreach (var doItem in Items.Where(doItem => doItem.ItemName == name))
					{
						doItem.Selected = box.Checked;
						var severItem = await session.LoadAsync<ToDoItem>(doItem.Id);
						severItem.Selected = box.Checked;
					}
					await session.SaveChangesAsync();
				}
			};

			//Finally return the view
			return view;
		}

		public override int Count
		{
			get { return Items.Count; }
		}

		public override ToDoItem this[int position]
		{
			get { return Items[position]; }
		}
	}
}