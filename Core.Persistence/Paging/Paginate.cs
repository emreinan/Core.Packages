using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Persistence.Paging;

public class Paginate<T>
{
	public Paginate()
	{
		Items = Array.Empty<T>();
	}

	public int Index { get; set; } // Page number
	public int Size { get; set; } // Number of items per page
	public int Count { get; set; } // Number of items in the current page
	public int Pages { get; set; }  // Total number of pages
	public IList<T> Items { get; set; }

	public bool HasPrevious => Index > 0; // Check if there is a previous page
	public bool HasNext => Index + 1 < Pages; // Check if there is a next page

}
