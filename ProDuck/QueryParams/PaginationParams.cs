namespace ProDuck.QueryParams
{
    public class PaginationParams
    {
        private const int _maxPageSize = 100;
        private int _pageSize = 10;

        public int Page { get; set; } = 1;

        public int PageSize
        {
            get
            {
                return _pageSize;
            }

            set
            {
                if (value > _maxPageSize) _pageSize = _maxPageSize;
                else _pageSize = value;
            }
        }
    }
}
