using System;
using System.Collections.Generic;
using System.IO;
using Foundation;
using UIKit;
using Navigator.Pathfinding;
using Navigator.Primitives;

namespace Navigator.iOS {
    public class TableSource : UITableViewSource {
       
		public Room[] tableItems { get; set; }

        protected string cellIdentifier = "TableCell";

		ViewController _owner;
		public TableSource (ViewController owner, List<Room> items)
        {
			tableItems = items.ToArray();
			_owner = owner;
        }
        public override nint RowsInSection (UITableView tableview, nint section)
        {
            return tableItems.Length;
        }
        public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
        {
            // request a recycled cell to save memory
            UITableViewCell cell = tableView.DequeueReusableCell (cellIdentifier);
            // if there are no cells to reuse, create a new one
			if (cell == null)
				cell = new UITableViewCell (UITableViewCellStyle.Default, cellIdentifier);
			cell.TextLabel.Text = tableItems [indexPath.Row].Name;
            return cell;
        }

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			var roomPosition = tableItems [indexPath.Row].Position;
			_owner.showContextMenu (roomPosition.X, roomPosition.Y);

			tableView.DeselectRow (indexPath, true);
		}
    }


        
}