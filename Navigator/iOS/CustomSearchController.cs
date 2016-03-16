using System;
using UIKit;
using Navigator.Pathfinding;
using System.Collections.Generic;
using Navigator.Primitives;

namespace Navigator.iOS
{
	public class CustomSearchController
	{
		UISearchBar _searchBar;
		UITableView _searchPredictionTable;

		TableSource tableSource;
		public CustomSearchController (ViewController owner, UISearchBar searchBar, UITableView searchPredictionTable, List<Room> rooms)
		{
			_searchBar = searchBar;
			_searchPredictionTable = searchPredictionTable;
			owner.InvokeOnMainThread (delegate() {
				_searchPredictionTable.Alpha = 0;
			});

			tableSource = new TableSource (owner, rooms);
			_searchPredictionTable.Source = tableSource;

			_searchBar.TextChanged += (sender, e) => {
				owner.InvokeOnMainThread (delegate() {
                    tableSource.tableItems = rooms.FindAll ((room) => room.Name.ToLower().Contains (e.SearchText.ToLower())).ToArray ();
					_searchPredictionTable.ReloadData();
				});
			};

			_searchBar.CancelButtonClicked += (sender, e) => {
				_searchBar.ShowsCancelButton = false;
				_searchBar.ResignFirstResponder();
			};

			_searchBar.OnEditingStarted += (sender, e) => {
				_searchBar.ShowsCancelButton = true;
				_searchPredictionTable.Alpha = 1;
			};
            _searchBar.OnEditingStopped += (sender, e) => {
                _searchPredictionTable.Alpha = 0;
                _searchBar.ResignFirstResponder ();

            };

		}
	}
}

