using System;
using System.Collections.Generic;

namespace GreenhouseMonitoring.ViewModels;

public class PageViewModel<T>
{
    public PageViewModel(int totalItems, int pageNumber, int pageSize)
    {
        TotalItems = totalItems;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
    }

    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public List<T> Items { get; set; } = new();
}