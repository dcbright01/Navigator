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

            cell.TextLabel.TextColor = UIColor.White;
            switch (tableItems [indexPath.Row].Type) {

            case "Lab":
                cell.BackgroundColor = UIColor.FromRGB (158, 30, 98);
                break;
            case "Utility":
                cell.BackgroundColor = UIColor.FromRGB (164, 164, 164);
                break;
            case "Office":
                cell.BackgroundColor = UIColor.FromRGB (11, 39, 63);
                break;
            case "Toilet":
                cell.BackgroundColor = UIColor.FromRGB (191, 185, 73);
                break;
            case "Stairs":
                cell.BackgroundColor = UIColor.FromRGB (208, 74, 45);
                break;

            }

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