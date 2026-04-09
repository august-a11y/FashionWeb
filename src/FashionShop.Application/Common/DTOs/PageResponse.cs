using FashionShop.Domain.Common;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Common.DTOs
{
    public class PageResponse<T> where T : class
    {
        public List<T> Items {  get; set; }
        public int Total { get; set; }
        public PageSize PageSize { get; set; }
        public PageIndex PageIndex { get; set; }
        public int TotalPages { get;set; }
        public PageResponse(List<T> items, int total, int pageSize, int pageIndex)
        {
            Items = items;
            Total = total;
            this.PageSize = new PageSize(pageSize);
            this.PageIndex = new PageIndex(pageIndex);
            TotalPages = (int)Math.Ceiling((double)total / (double)PageSize._value);
        }
    }
    public class PageSize
    {
        public int? _value { get; set; }

        private static int _maxPageSize = 100;
        private static int _defaultPageSize = 20;

        public PageSize(int? value)
        {

            if (value <= 0 || value is null)
            {
                _value = _defaultPageSize;
            }
            else if (value > _maxPageSize)
            {
                _value = _maxPageSize;
            }
            else
            {
                _value = value;
            }
        }
       
    }
    public class PageIndex
    {
        public int? _value
        {
            get; set;
        }
        private static int _defaultPageIndex = 1;
        public PageIndex(int? value)
        {
            if (value <= 0 || value is null)
            {
                _value = _defaultPageIndex;
            }
            else
            {
                _value = value;
            }
        }
    }
    }
